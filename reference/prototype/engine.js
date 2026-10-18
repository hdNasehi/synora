// Synora — date, schedule, holiday and approval logic.
// Runs in the browser against sample data. No backend: everything here is simulated.

export const TODAY = '2026-10-19'; // fixed prototype date (Monday)
export const NOW_TIME = '10:24';

const DAY_MS = 86400000;
export const toDate = (iso) => new Date(iso + 'T00:00:00Z');
export const toIso = (dt) => dt.toISOString().slice(0, 10);
export const addDays = (iso, n) => toIso(new Date(toDate(iso).getTime() + n * DAY_MS));
export const dow = (iso) => toDate(iso).getUTCDay(); // 0 = Sunday
export const diffDays = (a, b) => Math.round((toDate(b) - toDate(a)) / DAY_MS);

import { t, fmt, fmtRange, fmtLong, fmtStamp, num, weekdayLong, weekdayShort } from './i18n.js';
export { fmt, fmtRange, fmtLong, fmtStamp, num };
export { t, tn, parts, monthOf, shiftMonth, weekdayMin, weekdayTwo, weekdayShort, weekStart, localDigits, isFa, isSolar, calendarName } from './i18n.js';
export const plural = (n, w) => `${num(n)} ${w}${n === 1 ? '' : 's'}`;

const minutes = (t) => { const [h, m] = t.split(':').map(Number); return h * 60 + m; };
export const hoursBetween = (a, b) => Math.max(0, (minutes(b) - minutes(a)) / 60);

// ---- schedules -----------------------------------------------------------
// days: index 0 = Sunday … 6 = Saturday → { start, end, hours } or null (rest day)
export function scheduleFor(state, employee, iso) {
  const a = employee.schedules.find((s) => s.from <= iso && (!s.to || iso <= s.to)) || employee.schedules[0];
  return state.schedules.find((s) => s.id === a.scheduleId);
}
export const scheduleSummary = (sch) => {
  const on = sch.days.map((d, i) => (d ? i : null)).filter((i) => i !== null);
  const hours = sch.days.reduce((t, d) => t + (d ? d.hours : 0), 0);
  return { workDays: on.map((i) => weekdayShort(i)), count: on.length, hours };
};

// ---- holidays --------------------------------------------------------------
// A holiday only excludes a day when the employee's holiday group observes it.
export function holidayOn(state, employee, iso) {
  const h = state.holidays.find((x) => x.date === iso);
  if (!h) return null;
  const observed = !!h.observedBy[employee.holidayGroup];
  return { ...h, observed };
}

// ---- leave calculation ---------------------------------------------------
// Returns the working-day/hour cost of a request for this employee.
export function calculate(state, employee, { start, end, partial, from, to }) {
  const out = { valid: false, days: [], working: 0, hours: 0, excludedRest: [], excludedHolidays: [], ignoredHolidays: [], errors: [] };
  if (!start || !end) return out;
  if (end < start) { out.errors.push(t('End date is before the start date.')); return out; }
  if (diffDays(start, end) > 62) { out.errors.push(t('Requests are limited to 62 calendar days (sample rule).')); return out; }
  if (partial && start !== end) { out.errors.push(t('A partial-day request must start and end on the same date.')); return out; }
  for (let iso = start; iso <= end; iso = addDays(iso, 1)) {
    const sch = scheduleFor(state, employee, iso);
    const day = sch.days[dow(iso)];
    const hol = holidayOn(state, employee, iso);
    if (!day) {
      out.days.push({ iso, kind: 'rest', label: t('{day} — rest day ({code})', { day: weekdayLong(dow(iso)), code: sch.code }) });
      out.excludedRest.push(iso);
      if (hol) out.ignoredHolidays.push(hol);
      continue;
    }
    if (hol && hol.observed) {
      out.days.push({ iso, kind: hol.kind, label: t(hol.name) });
      out.excludedHolidays.push(hol);
      continue;
    }
    if (hol && !hol.observed) out.ignoredHolidays.push(hol);
    let hrs = day.hours, frac = 1;
    if (partial) {
      const h = hoursBetween(from || day.start, to || day.end);
      if (h <= 0) { out.errors.push(t('End time must be after the start time.')); return out; }
      if (minutes(from) < minutes(day.start) || minutes(to) > minutes(day.end)) out.errors.push(t('Times must fall within working hours ({from}–{to}).', { from: day.start, to: day.end }));
      hrs = Math.min(h, day.hours);
      frac = Math.round((hrs / day.hours) * 100) / 100;
    }
    out.days.push({ iso, kind: 'work', hours: hrs, frac, label: partial ? `${from}–${to}` : `${day.start}–${day.end}` });
    out.working += frac;
    out.hours += hrs;
  }
  out.working = Math.round(out.working * 100) / 100;
  if (out.working === 0 && !out.errors.length) out.errors.push(t('The selected dates contain no working days for this employee.'));
  out.valid = out.errors.length === 0;
  return out;
}

// ---- requests & approval --------------------------------------------------
const STATUS_TEXT = { draft: 'Draft', pending_supervisor: 'Pending Supervisor Approval', pending_manager: 'Pending Manager Approval', approved: 'Approved', rejected: 'Rejected', cancelled: 'Cancelled' };
export const STATUS = {};
Object.keys(STATUS_TEXT).forEach((k) => Object.defineProperty(STATUS, k, { enumerable: true, get: () => t(STATUS_TEXT[k]) }));

export function statusOf(req) {
  if (req.state === 'draft') return 'draft';
  if (req.state === 'cancelled') return 'cancelled';
  if (req.steps.some((s) => s.state === 'rejected')) return 'rejected';
  const open = req.steps.find((s) => s.state !== 'done' && s.state !== 'skipped');
  if (!open) return 'approved';
  return open.kind === 'manager' ? 'pending_manager' : 'pending_supervisor';
}
export const currentStep = (req) => req.steps.find((s) => s.state === 'active');

const RULE_TEXT = {
  any: ['Any one supervisor', 'Any one', 'The first assigned supervisor to approve completes this step.'],
  all: ['All supervisors', 'All required', 'Every assigned supervisor must approve, in any order.'],
  sequential: ['Sequential', 'In order', 'Supervisors approve one after another, in the configured order.'],
  none: ['No supervisor step', 'None', 'The request goes straight to the final manager.'],
};
export const RULES = {};
Object.keys(RULE_TEXT).forEach((k) => Object.defineProperty(RULES, k, { enumerable: true, get: () => ({ label: t(RULE_TEXT[k][0]), short: t(RULE_TEXT[k][1]), help: t(RULE_TEXT[k][2]) }) }));

// Builds the steps for a new request from the employee's assignment and the workflow.
export function buildSteps(state, employee, type) {
  const lt = state.leaveTypes.find((l) => l.id === type);
  if (lt.approval === 'notify') {
    return [{ id: 'n1', kind: 'notify', label: t('Supervisor notified'), rule: 'none', approvers: employee.supervisors, decisions: [], state: 'done' }];
  }
  const steps = [];
  const sups = employee.supervisors;
  if (sups.length && employee.rule !== 'none') {
    if (employee.rule === 'sequential') {
      sups.forEach((id, i) => steps.push({ id: 's' + (i + 1), kind: 'supervisor', label: t('Supervisor {i} of {n}', { i: i + 1, n: sups.length }), rule: 'sequential', approvers: [id], decisions: [], state: i === 0 ? 'active' : 'waiting' }));
    } else {
      steps.push({ id: 's1', kind: 'supervisor', label: employee.rule === 'all' ? t('Supervisors — all required') : t('Supervisor — any one'), rule: employee.rule, approvers: [...sups], decisions: [], state: 'active' });
    }
  }
  steps.push({ id: 'm', kind: 'manager', label: t('Final manager approval'), rule: 'manager', approvers: [employee.managerId], decisions: [], state: steps.length ? 'waiting' : 'active' });
  return steps;
}

// Who can act on the active step right now (includes substitutes for absent approvers).
export function actorsFor(state, step) {
  if (!step || step.state !== 'active') return [];
  const decided = step.decisions.map((d) => d.by);
  const ids = step.approvers.filter((a) => !decided.includes(a));
  const subs = ids.map((id) => state.substitutes.find((s) => s.for === id && s.from <= TODAY && TODAY <= s.to)).filter(Boolean).map((s) => s.by);
  return [...new Set([...ids, ...subs])];
}
export const canAct = (state, req, userId) => actorsFor(state, currentStep(req)).includes(userId);

export function decide(state, req, userId, decision, comment) {
  const at = `${TODAY} ${NOW_TIME}`;
  const steps = req.steps.map((s) => ({ ...s, decisions: [...s.decisions] }));
  const i = steps.findIndex((s) => s.state === 'active');
  const step = steps[i];
  const sub = state.substitutes.find((s) => s.by === userId && step.approvers.includes(s.for) && !step.approvers.includes(userId));
  step.decisions.push({ by: userId, decision, at, comment, onBehalfOf: sub ? sub.for : undefined });
  if (decision === 'rejected') {
    step.state = 'rejected';
    steps.slice(i + 1).forEach((s) => (s.state = 'skipped'));
  } else {
    const approvedBy = step.decisions.filter((d) => d.decision === 'approved').map((d) => d.onBehalfOf || d.by);
    const complete = step.rule === 'all' ? step.approvers.every((a) => approvedBy.includes(a)) : true;
    if (complete) {
      step.state = 'done';
      if (steps[i + 1]) steps[i + 1].state = 'active';
    }
  }
  return { ...req, steps };
}

// ---- balances ---------------------------------------------------------------
// Consumed = approved annual leave; reserved = pending annual leave. Each request counts once.
export function balance(state, employeeId, year = 2026) {
  const emp = state.employees.find((e) => e.id === employeeId);
  const pol = state.policies.find((p) => p.id === emp.policyId);
  const mine = state.requests.filter((r) => r.employeeId === employeeId && r.type === 'annual' && r.start.startsWith(String(year)));
  const consumed = mine.filter((r) => statusOf(r) === 'approved').reduce((t, r) => t + r.amount, 0);
  const taken = mine.filter((r) => statusOf(r) === 'approved' && r.end < TODAY).reduce((t, r) => t + r.amount, 0);
  const reserved = mine.filter((r) => ['pending_supervisor', 'pending_manager'].includes(statusOf(r))).reduce((t, r) => t + r.amount, 0);
  const adj = state.adjustments.filter((a) => a.employeeId === employeeId && a.year === year);
  const carry = adj.filter((a) => a.kind === 'carry').reduce((t, a) => t + a.amount, 0);
  const adjustments = adj.filter((a) => a.kind === 'adjustment').reduce((t, a) => t + a.amount, 0);
  const entitlement = Math.round(pol.entitlement * (emp.fte ?? 1) * 100) / 100;
  const accrued = Math.round(entitlement * (9 / 12) * 10) / 10; // through end of Sep (monthly accrual, info only)
  const total = entitlement + carry + adjustments;
  const available = Math.round((total - consumed - reserved) * 100) / 100;
  return { unit: pol.unit, entitlement, accrued, carry, adjustments, consumed, taken, scheduled: consumed - taken, reserved, total, available, policy: pol };
}

export function sickSummary(state, employeeId) {
  const rs = state.requests.filter((r) => r.employeeId === employeeId && r.type === 'sick' && statusOf(r) !== 'cancelled');
  return { count: rs.length, days: rs.reduce((t, r) => t + r.amount, 0) };
}
export function unpaidSummary(state, employeeId) {
  const rs = state.requests.filter((r) => r.employeeId === employeeId && r.type === 'unpaid');
  return {
    used: rs.filter((r) => statusOf(r) === 'approved').reduce((t, r) => t + r.amount, 0),
    pending: rs.filter((r) => statusOf(r).startsWith('pending')).reduce((t, r) => t + r.amount, 0),
  };
}

// Working days between creation and today (Mon–Fri) for SLA tracking.
export function waitingDays(req) {
  const created = req.submittedAt.slice(0, 10);
  let n = 0;
  for (let iso = addDays(created, 1); iso <= TODAY; iso = addDays(iso, 1)) if (dow(iso) > 0 && dow(iso) < 6) n++;
  return n;
}

export const overlaps = (a1, b1, a2, b2) => a1 <= b2 && a2 <= b1;

// Team members of the same team absent (approved or pending) on any day of the request.
export function availability(state, req) {
  const emp = state.employees.find((e) => e.id === req.employeeId);
  const team = state.employees.filter((e) => e.team === emp.team && e.active && e.id !== emp.id);
  const others = state.requests.filter((r) => r.id !== req.id && team.some((t) => t.id === r.employeeId) && ['approved', 'pending_supervisor', 'pending_manager'].includes(statusOf(r)) && overlaps(r.start, r.end, req.start, req.end));
  let peak = 0, peakDay = null;
  for (let iso = req.start; iso <= req.end; iso = addDays(iso, 1)) {
    const n = new Set(others.filter((r) => r.start <= iso && iso <= r.end).map((r) => r.employeeId)).size;
    if (n > peak) { peak = n; peakDay = iso; }
  }
  const t = state.teams.find((x) => x.id === emp.team);
  const size = team.length + 1;
  const present = size - peak - 1;
  return { team: t, size, peak, peakDay, present, warn: present < t.minPresent, people: [...new Set(others.map((r) => r.employeeId))] };
}
