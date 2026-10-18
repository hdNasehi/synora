// Synora — realistic sample data for Noordkade B.V. (fictional company).
// Every policy value here is a configurable SAMPLE, not a confirmed legal rule.
import { calculate, buildSteps } from './engine.js';

const sch = (id, code, name, desc, map, group) => ({ id, code, name, desc, group, days: map });
const D = (start, end) => ({ start, end, hours: Math.round(((+end.slice(0, 2) * 60 + +end.slice(3)) - (+start.slice(0, 2) * 60 + +start.slice(3)) - 30) / 60 * 100) / 100 });
const day = D('09:00', '17:30'); // 8h with 30 min unpaid break

export const schedules = [
  sch('sch-a', 'A', 'Schedule A — Mon–Fri', 'Standard office week. Saturday and Sunday are rest days.', [null, day, day, day, day, day, null]),
  sch('sch-b', 'B', 'Schedule B — Sun–Thu', 'International support desk. Friday and Saturday are rest days.', [D('08:00', '16:30'), D('08:00', '16:30'), D('08:00', '16:30'), D('08:00', '16:30'), D('08:00', '16:30'), null, null]),
  sch('sch-p', 'P', 'Schedule P — Mon–Thu (part-time)', 'Four-day week, 32 hours. Friday, Saturday and Sunday are rest days.', [null, day, day, day, day, null, null]),
];

export const holidayGroups = [
  { id: 'nl-office', name: 'NL office staff', desc: 'Employees on Schedule A or P at the Rotterdam office.' },
  { id: 'support-desk', name: 'International support desk', desc: 'Employees on Schedule B serving MENA customers.' },
];

const H = (id, date, name, local, kind, office, desk, note) => ({ id, date, name, local, kind, observedBy: { 'nl-office': office, 'support-desk': desk }, note });
export const holidays = [
  H('h26-01', '2026-01-01', "New Year's Day", 'Nieuwjaarsdag', 'public', true, true),
  H('h26-02', '2026-04-03', 'Good Friday', 'Goede Vrijdag', 'public', false, false, 'Not a company rest day under the sample CAO.'),
  H('h26-03', '2026-04-05', 'Easter Sunday', 'Eerste Paasdag', 'public', true, true),
  H('h26-04', '2026-04-06', 'Easter Monday', 'Tweede Paasdag', 'public', true, true),
  H('h26-05', '2026-04-27', "King's Day", 'Koningsdag', 'public', true, true),
  H('h26-06', '2026-05-05', 'Liberation Day', 'Bevrijdingsdag', 'public', false, false, 'Sample policy: observed in lustrum years only (next: 2030).'),
  H('h26-07', '2026-05-14', 'Ascension Day', 'Hemelvaartsdag', 'public', true, true),
  H('h26-c1', '2026-05-15', 'Bridge day', 'Brugdag', 'company', true, false, 'Office closed. Support desk staffed.'),
  H('h26-08', '2026-05-24', 'Whit Sunday', 'Eerste Pinksterdag', 'public', true, true),
  H('h26-09', '2026-05-25', 'Whit Monday', 'Tweede Pinksterdag', 'public', true, true),
  H('h26-c2', '2026-11-06', 'Noordkade Day', 'Bedrijfsdag', 'company', true, true, 'Annual company day. All staff off.'),
  H('h26-10', '2026-12-25', 'Christmas Day', 'Eerste Kerstdag', 'public', true, true),
  H('h26-11', '2026-12-26', 'Boxing Day', 'Tweede Kerstdag', 'public', true, true),
  H('h27-01', '2027-01-01', "New Year's Day", 'Nieuwjaarsdag', 'public', true, true),
  H('h27-02', '2027-03-26', 'Good Friday', 'Goede Vrijdag', 'public', false, false, 'Not a company rest day under the sample CAO.'),
  H('h27-03', '2027-03-28', 'Easter Sunday', 'Eerste Paasdag', 'public', true, true),
  H('h27-04', '2027-03-29', 'Easter Monday', 'Tweede Paasdag', 'public', true, true),
  H('h27-05', '2027-04-27', "King's Day", 'Koningsdag', 'public', true, true),
  H('h27-06', '2027-05-05', 'Liberation Day', 'Bevrijdingsdag', 'public', false, false, 'Sample policy: observed in lustrum years only (next: 2030).'),
  H('h27-07', '2027-05-06', 'Ascension Day', 'Hemelvaartsdag', 'public', true, true),
  H('h27-08', '2027-05-16', 'Whit Sunday', 'Eerste Pinksterdag', 'public', true, true),
  H('h27-09', '2027-05-17', 'Whit Monday', 'Tweede Pinksterdag', 'public', true, true),
  H('h27-10', '2027-12-25', 'Christmas Day', 'Eerste Kerstdag', 'public', true, true),
  H('h27-11', '2027-12-26', 'Boxing Day', 'Tweede Kerstdag', 'public', true, true),
];

export const leaveTypes = [
  { id: 'annual', name: 'Annual leave', short: 'Annual', consumes: true, approval: 'workflow', attachment: 'none', desc: 'Paid vacation. Deducted from the annual leave balance.' },
  { id: 'sick', name: 'Sick leave', short: 'Sick', consumes: false, approval: 'notify', attachment: 'optional', desc: 'Registered absence. Tracked separately; never deducted from annual leave. No medical details are stored.' },
  { id: 'unpaid', name: 'Unpaid leave', short: 'Unpaid', consumes: false, approval: 'workflow', attachment: 'none', desc: 'Unpaid absence. Requires approval; never deducted from annual leave.' },
];

export const policies = [
  {
    id: 'pol-std', name: 'Standard — full-time', unit: 'days', entitlement: 25, hoursPerDay: 8,
    statedAs: '“One month per year” (business wording)', interpretation: '25 working days = 5 working weeks × 5 days. Pro-rated by FTE.',
    accrual: 'monthly', advance: true, carryMax: 5, carryExpiry: '2027-07-01', minNotice: 2, maxConsecutive: 20, from: '2026-01-01',
  },
  {
    id: 'pol-pt', name: 'Part-time — hours based', unit: 'hours', entitlement: 160, hoursPerDay: 8,
    statedAs: '“One month per year”', interpretation: '4 weeks × 32 h + 1 week × 32 h = 160 hours.',
    accrual: 'monthly', advance: true, carryMax: 40, carryExpiry: '2027-07-01', minNotice: 2, maxConsecutive: 20, from: '2026-01-01',
  },
];

export const teams = [
  { id: 'cs', name: 'Customer Success', dept: 'Operations', minPresent: 5 },
  { id: 'pl', name: 'Planning & Logistics', dept: 'Operations', minPresent: 4 },
  { id: 'fin', name: 'Finance', dept: 'Finance', minPresent: 1 },
  { id: 'eng', name: 'Engineering', dept: 'Technology', minPresent: 2 },
  { id: 'hr', name: 'People & HR', dept: 'People', minPresent: 1 },
  { id: 'mgmt', name: 'Management', dept: 'Management', minPresent: 0 },
];

const E = (id, name, title, team, extra) => ({
  id, name, title, team, active: true, policyId: 'pol-std', fte: 1, holidayGroup: 'nl-office',
  schedules: [{ scheduleId: 'sch-a', from: '2024-01-01', to: null }], supervisors: [], rule: 'none', managerId: 'e13',
  roles: ['employee'], startDate: '2023-03-01', email: name.toLowerCase().replace(/é/g, 'e').replace(/[^a-z ]/g, '').trim().split(' ').join('.') + '@noordkade.nl', ...extra,
});

export const employees = [
  E('e01', 'Sanne de Vries', 'Customer Success Specialist', 'cs', { no: 'NK-1042', supervisors: ['e02', 'e03'], rule: 'all', startDate: '2022-09-01', assignFrom: '2025-04-01' }),
  E('e02', 'Daan Bakker', 'Team Lead, Customer Success', 'cs', { no: 'NK-0871', supervisors: ['e03'], rule: 'any', roles: ['employee', 'supervisor'], startDate: '2020-02-03' }),
  E('e03', 'Priya Raman', 'Operations Lead', 'cs', { no: 'NK-0655', roles: ['employee', 'supervisor'], startDate: '2019-06-17' }),
  E('e04', 'Joris Meijer', 'Customer Success Specialist', 'cs', { no: 'NK-1188', supervisors: ['e02', 'e03'], rule: 'any', startDate: '2024-01-08' }),
  E('e05', 'Omar Haddad', 'Support Specialist, MENA desk', 'cs', { no: 'NK-1203', supervisors: ['e02'], rule: 'any', holidayGroup: 'support-desk', schedules: [{ scheduleId: 'sch-b', from: '2024-03-01', to: null }], startDate: '2024-03-01' }),
  E('e06', 'Mila Bos', 'Customer Success Specialist', 'cs', { no: 'NK-1121', supervisors: ['e02'], rule: 'any', startDate: '2023-05-15' }),
  E('e07', 'Lucas Peeters', 'Onboarding Specialist', 'cs', { no: 'NK-1250', supervisors: ['e02'], rule: 'any', startDate: '2025-02-03' }),
  E('e08', 'Femke Smit', 'Planner', 'pl', { no: 'NK-0990', supervisors: ['e09'], rule: 'any', policyId: 'pol-pt', fte: 1, schedules: [{ scheduleId: 'sch-p', from: '2023-01-01', to: null }], startDate: '2021-11-01' }),
  E('e09', 'Bram Willems', 'Team Lead, Planning', 'pl', { no: 'NK-0712', roles: ['employee', 'supervisor'], startDate: '2019-10-01' }),
  E('e10', 'Ahmed El Amrani', 'Logistics Planner', 'pl', { no: 'NK-1077', supervisors: ['e09', 'e03'], rule: 'sequential', startDate: '2022-11-14' }),
  E('e11', 'Tim van Dijk', 'Logistics Planner', 'pl', { no: 'NK-1145', supervisors: ['e09'], rule: 'any', startDate: '2023-08-21' }),
  E('e12', 'Noor Hendriks', 'Transport Coordinator', 'pl', { no: 'NK-1199', supervisors: ['e09'], rule: 'any', startDate: '2024-02-12' }),
  E('e13', 'Eva Jansen', 'Head of Operations', 'mgmt', { no: 'NK-0402', managerId: 'e17', roles: ['employee', 'manager'], startDate: '2018-04-03' }),
  E('e14', 'Lotte Visser', 'HR Business Partner', 'hr', { no: 'NK-0588', managerId: 'e17', roles: ['employee', 'hr'], startDate: '2019-01-07' }),
  E('e15', 'Chloé Martin', 'Software Engineer', 'eng', { no: 'NK-1210', managerId: 'e17', startDate: '2024-06-03' }),
  E('e16', 'Kees Mulder', 'Accountant', 'fin', { no: 'NK-0833', managerId: 'e17', active: false, endDate: '2026-08-31', startDate: '2020-05-04' }),
  E('e17', 'Marc Dekker', 'Managing Director', 'mgmt', { no: 'NK-0101', managerId: null, roles: ['employee', 'manager'], startDate: '2015-01-05' }),
];

export const personas = {
  employee: { id: 'e01', label: 'Employee' },
  supervisor: { id: 'e02', label: 'Supervisor' },
  manager: { id: 'e13', label: 'Manager' },
  hr: { id: 'e14', label: 'HR admin' },
};

export const substitutes = [
  { for: 'e02', by: 'e03', from: '2026-12-28', to: '2027-01-01', note: 'Daan on annual leave' },
  { for: 'e09', by: 'e03', from: '2026-10-19', to: '2026-10-23', note: 'Bram on annual leave' },
];

export const adjustments = [
  { id: 'adj-01', employeeId: 'e01', year: 2026, kind: 'carry', amount: 3, reason: 'Carry-over from 2025 (max 5 days, expires 1 Jul 2027)', by: 'system', at: '2026-01-01 00:05' },
  { id: 'adj-02', employeeId: 'e01', year: 2026, kind: 'adjustment', amount: 1, reason: 'Long-service day (5+ years incl. traineeship) — policy LS-2', by: 'e14', at: '2026-01-05 09:12' },
  { id: 'adj-03', employeeId: 'e04', year: 2026, kind: 'carry', amount: 2, reason: 'Carry-over from 2025', by: 'system', at: '2026-01-01 00:05' },
  { id: 'adj-04', employeeId: 'e10', year: 2026, kind: 'adjustment', amount: -1, reason: 'Correction: 4 May booked as sick in error, re-registered as annual leave', by: 'e14', at: '2026-05-12 15:40' },
  { id: 'adj-05', employeeId: 'e05', year: 2026, kind: 'carry', amount: 4.5, reason: 'Carry-over from 2025', by: 'system', at: '2026-01-01 00:05' },
  { id: 'adj-06', employeeId: 'e08', year: 2026, kind: 'carry', amount: 12, reason: 'Carry-over from 2025 (hours)', by: 'system', at: '2026-01-01 00:05' },
];

// [id, employee, type, start, end, submittedAt, decisions, extra]
// decisions: list of [stepIndex, by, 'approved'|'rejected', at, comment]
const R = [
  ['LR-2026-0045', 'e01', 'annual', '2026-02-16', '2026-02-20', '2026-01-12 11:03', [[0, 'e02', 'approved', '2026-01-12 14:20'], [0, 'e03', 'approved', '2026-01-13 09:02'], [1, 'e13', 'approved', '2026-01-13 16:45']], { reason: 'Ski trip — Krokus week' }],
  ['LR-2026-0122', 'e01', 'annual', '2026-04-01', '2026-04-03', '2026-03-02 10:15', [[0, 'e02', 'approved', '2026-03-02 13:00']], { state: 'cancelled', cancelledAt: '2026-03-20 08:31', cancelReason: 'Plans changed.' }],
  ['LR-2026-0131', 'e01', 'sick', '2026-03-02', '2026-03-03', '2026-03-02 08:12', [], { reason: '' }],
  ['LR-2026-0150', 'e01', 'annual', '2026-05-11', '2026-05-15', '2026-03-30 09:40', [[0, 'e02', 'approved', '2026-03-30 11:00'], [0, 'e03', 'approved', '2026-03-31 08:55'], [1, 'e13', 'approved', '2026-04-01 10:10']], { reason: 'Ascension week' }],
  ['LR-2026-0198', 'e01', 'unpaid', '2026-06-22', '2026-06-22', '2026-06-01 15:22', [[0, 'e02', 'rejected', '2026-06-02 09:30', 'Quarter-end close on 22 June needs full coverage. Happy to approve another date that week.']], { reason: 'Moving house' }],
  ['LR-2026-0240', 'e01', 'annual', '2026-08-03', '2026-08-07', '2026-05-18 12:05', [[0, 'e02', 'approved', '2026-05-18 16:00'], [0, 'e03', 'approved', '2026-05-19 08:40'], [1, 'e13', 'approved', '2026-05-19 14:12']], { reason: 'Summer holiday' }],
  ['LR-2026-0296', 'e01', 'annual', '2026-09-18', '2026-09-18', '2026-09-10 09:00', [[0, 'e02', 'approved', '2026-09-10 10:30'], [0, 'e03', 'approved', '2026-09-10 11:15'], [1, 'e13', 'approved', '2026-09-10 15:02']], { partial: true, from: '13:30', to: '17:30', reason: 'Afternoon appointment' }],
  ['LR-2026-0301', 'e01', 'sick', '2026-09-14', '2026-09-15', '2026-09-14 07:58', [], {}],
  ['LR-2026-0388', 'e01', 'annual', '2026-12-28', '2026-12-31', '2026-09-30 10:44', [[0, 'e02', 'approved', '2026-09-30 13:10'], [0, 'e03', 'approved', '2026-10-01 09:20'], [1, 'e13', 'approved', '2026-10-01 11:05', 'Enjoy the break.']], { reason: 'Christmas with family in Groningen' }],
  ['LR-2026-0412', 'e01', 'annual', '2026-11-23', '2026-11-24', '2026-10-15 09:31', [[0, 'e02', 'approved', '2026-10-15 14:02', 'Fine for CS coverage.']], { reason: 'Long weekend' }],
  ['LR-2027-D001', 'e01', 'annual', '2027-01-04', '2027-01-08', '2026-10-12 20:15', [], { state: 'draft', reason: '' }],

  ['LR-2026-0420', 'e04', 'annual', '2026-11-02', '2026-11-06', '2026-10-16 16:48', [], { reason: 'Autumn trip to Lisbon' }],
  ['LR-2026-0405', 'e05', 'annual', '2026-11-05', '2026-11-08', '2026-10-12 08:20', [], { reason: 'Family visit' }],
  ['LR-2026-0425', 'e07', 'annual', '2026-10-20', '2026-10-20', '2026-10-19 08:47', [], { partial: true, from: '09:00', to: '13:00', reason: 'Notary appointment' }],
  ['LR-2026-0418', 'e06', 'annual', '2026-11-30', '2026-12-04', '2026-10-14 13:05', [], { reason: 'Week off' }],
  ['LR-2026-0399', 'e10', 'annual', '2026-12-14', '2026-12-18', '2026-10-07 10:00', [[0, 'e09', 'approved', '2026-10-08 09:14'], [1, 'e03', 'approved', '2026-10-12 11:37', 'Covered by Tim for carrier calls.']], { reason: 'Visiting family in Tangier' }],
  ['LR-2026-0402', 'e12', 'unpaid', '2027-01-04', '2027-01-08', '2026-10-09 14:22', [[0, 'e09', 'approved', '2026-10-13 10:05']], { reason: 'Extended trip after the holidays' }],
  ['LR-2026-0410', 'e11', 'annual', '2026-12-17', '2026-12-24', '2026-10-13 09:10', [[0, 'e09', 'approved', '2026-10-14 08:30', 'Overlaps Ahmed on 17–18 Dec; manager to confirm planning cover.']], { reason: 'Christmas holiday' }],
  ['LR-2026-0380', 'e03', 'annual', '2026-10-26', '2026-10-30', '2026-09-21 09:00', [[0, 'e13', 'approved', '2026-09-21 12:00']], { reason: 'Autumn break' }],
  ['LR-2026-0391', 'e02', 'annual', '2026-12-28', '2027-01-01', '2026-10-02 10:30', [[0, 'e03', 'approved', '2026-10-02 12:10'], [1, 'e13', 'approved', '2026-10-02 16:40']], { reason: 'Year-end' }],
  ['LR-2026-0370', 'e09', 'annual', '2026-10-19', '2026-10-23', '2026-09-01 11:11', [[0, 'e13', 'approved', '2026-09-02 09:00']], { reason: 'Autumn holiday' }],
  ['LR-2026-0428', 'e08', 'sick', '2026-10-19', '2026-10-20', '2026-10-19 07:40', [], {}],
  ['LR-2026-0360', 'e15', 'annual', '2026-11-09', '2026-11-13', '2026-09-15 15:00', [[0, 'e17', 'approved', '2026-09-16 10:00']], {}],
  ['LR-2026-0395', 'e13', 'annual', '2026-11-16', '2026-11-20', '2026-10-05 09:00', [[0, 'e17', 'approved', '2026-10-05 17:30']], {}],
  ['LR-2026-0350', 'e11', 'annual', '2026-08-24', '2026-08-28', '2026-07-20 10:00', [[0, 'e09', 'approved', '2026-07-20 15:00'], [1, 'e13', 'rejected', '2026-07-21 09:40', 'Peak season — two planners already off that week. Please pick September.']], {}],
  ['LR-2026-0333', 'e04', 'annual', '2026-07-13', '2026-07-24', '2026-05-04 10:00', [[0, 'e03', 'approved', '2026-05-04 12:00'], [1, 'e13', 'approved', '2026-05-05 09:00']], {}],
  ['LR-2026-0340', 'e06', 'annual', '2026-08-10', '2026-08-21', '2026-05-11 10:00', [[0, 'e02', 'approved', '2026-05-11 12:00'], [1, 'e13', 'approved', '2026-05-12 09:00']], {}],
  ['LR-2026-0342', 'e05', 'annual', '2026-06-28', '2026-07-09', '2026-05-12 10:00', [[0, 'e02', 'approved', '2026-05-12 12:00'], [1, 'e13', 'approved', '2026-05-13 09:00']], {}],
  ['LR-2026-0344', 'e10', 'annual', '2026-07-27', '2026-08-07', '2026-05-14 10:00', [[0, 'e09', 'approved', '2026-05-14 12:00'], [1, 'e03', 'approved', '2026-05-14 15:00'], [2, 'e13', 'approved', '2026-05-15 09:00']], {}],
  ['LR-2026-0346', 'e08', 'annual', '2026-08-03', '2026-08-13', '2026-05-15 10:00', [[0, 'e09', 'approved', '2026-05-15 12:00'], [1, 'e13', 'approved', '2026-05-16 09:00']], {}],
  ['LR-2026-0348', 'e12', 'annual', '2026-09-07', '2026-09-11', '2026-06-15 10:00', [[0, 'e09', 'approved', '2026-06-15 12:00'], [1, 'e13', 'approved', '2026-06-16 09:00']], {}],
  ['LR-2026-0352', 'e07', 'annual', '2026-09-21', '2026-09-25', '2026-08-01 10:00', [[0, 'e02', 'approved', '2026-08-03 12:00'], [1, 'e13', 'approved', '2026-08-04 09:00']], {}],
  ['LR-2026-0354', 'e14', 'annual', '2026-08-17', '2026-08-28', '2026-06-01 10:00', [[0, 'e17', 'approved', '2026-06-01 12:00']], {}],
  ['LR-2026-0362', 'e07', 'annual', '2026-12-01', '2026-12-04', '2026-09-20 10:00', [[0, 'e02', 'approved', '2026-09-21 09:00'], [1, 'e13', 'approved', '2026-09-21 15:00']], { reason: 'Sinterklaas week' }],
  ['LR-2026-0364', 'e05', 'annual', '2026-11-29', '2026-12-03', '2026-09-22 10:00', [[0, 'e02', 'approved', '2026-09-22 12:00'], [1, 'e13', 'approved', '2026-09-23 09:00']], {}],
  ['LR-2026-0356', 'e04', 'sick', '2026-10-05', '2026-10-06', '2026-10-05 08:00', [], {}],
];

function buildRequests(state) {
  return R.map(([id, employeeId, type, start, end, submittedAt, decisions, extra]) => {
    const emp = state.employees.find((e) => e.id === employeeId);
    const calc = calculate(state, emp, { start, end, partial: !!extra.partial, from: extra.from, to: extra.to });
    const steps = buildSteps(state, emp, type);
    decisions.forEach(([si, by, decision, at, comment]) => {
      const s = steps[si];
      s.decisions.push({ by, decision, at, comment });
      if (decision === 'rejected') { s.state = 'rejected'; steps.slice(si + 1).forEach((x) => (x.state = 'skipped')); return; }
      const approvedBy = s.decisions.filter((d) => d.decision === 'approved').map((d) => d.by);
      if (s.rule !== 'all' || s.approvers.every((a) => approvedBy.includes(a))) { s.state = 'done'; if (steps[si + 1]) steps[si + 1].state = 'active'; }
    });
    const unit = state.policies.find((p) => p.id === emp.policyId).unit;
    return {
      id, employeeId, type, start, end, submittedAt, steps,
      state: 'submitted', partial: false, reason: '', attachment: type === 'sick' && id === 'LR-2026-0301' ? null : null,
      ...extra,
      amount: unit === 'hours' ? calc.hours : calc.working, hours: calc.hours, unit,
    };
  });
}

const baseAudit = [
  { at: '2026-10-15 14:02', by: 'e02', action: 'Approved request', target: 'LR-2026-0412 · Sanne de Vries', detail: 'Step 1 (all required): 1 of 2 approvals. Comment: “Fine for CS coverage.”', cat: 'approval' },
  { at: '2026-10-14 08:30', by: 'e09', action: 'Approved request', target: 'LR-2026-0410 · Tim van Dijk', detail: 'Step 1 complete → Final manager approval.', cat: 'approval' },
  { at: '2026-10-13 10:05', by: 'e09', action: 'Approved request', target: 'LR-2026-0402 · Noor Hendriks', detail: 'Step 1 complete → Final manager approval.', cat: 'approval' },
  { at: '2026-10-12 11:37', by: 'e03', action: 'Approved request', target: 'LR-2026-0399 · Ahmed El Amrani', detail: 'Sequential step 2 of 2 complete → Final manager approval.', cat: 'approval' },
  { at: '2026-10-01 11:05', by: 'e13', action: 'Final approval', target: 'LR-2026-0388 · Sanne de Vries', detail: 'Request approved. 4 days consumed from 2026 balance.', cat: 'approval' },
  { at: '2026-09-28 16:20', by: 'e14', action: 'Changed holiday observance', target: 'Noordkade Day · 6 Nov 2026', detail: 'International support desk: not observed → observed.', cat: 'config' },
  { at: '2026-07-21 09:40', by: 'e13', action: 'Rejected request', target: 'LR-2026-0350 · Tim van Dijk', detail: 'Reason: “Peak season — two planners already off that week. Please pick September.”', cat: 'approval' },
  { at: '2026-05-12 15:40', by: 'e14', action: 'Balance adjustment −1.0 day', target: 'Ahmed El Amrani · 2026', detail: 'Correction: 4 May booked as sick in error, re-registered as annual leave.', cat: 'balance' },
  { at: '2026-04-02 10:00', by: 'e14', action: 'Changed approval rule', target: 'Sanne de Vries', detail: 'Supervisors Daan Bakker, Priya Raman · rule: any one → all required. Effective 1 Apr 2026.', cat: 'config' },
  { at: '2026-01-05 09:12', by: 'e14', action: 'Balance adjustment +1.0 day', target: 'Sanne de Vries · 2026', detail: 'Long-service day (5+ years incl. traineeship) — policy LS-2.', cat: 'balance' },
  { at: '2026-01-01 00:05', by: 'system', action: 'Carry-over posted', target: '14 employees · 2025 → 2026', detail: 'Remaining 2025 balances carried over, capped at 5 days / 40 hours.', cat: 'balance' },
];

export function initialState() {
  const s = { schedules, holidays, holidayGroups, leaveTypes, policies, teams, employees, substitutes, adjustments };
  return { ...s, requests: buildRequests(s), audit: baseAudit, notifications: [
    { id: 'n1', at: '2026-10-15 14:02', text: 'Daan Bakker approved LR-2026-0412 (1 of 2 supervisors).', for: 'e01', read: false },
    { id: 'n2', at: '2026-10-01 11:05', text: 'Your Christmas leave (28–31 Dec) was approved.', for: 'e01', read: true },
    { id: 'n3', at: '2026-10-19 08:47', text: 'Lucas Peeters requested a half day on 20 Oct.', for: 'e02', read: false },
    { id: 'n4', at: '2026-10-16 16:48', text: 'Joris Meijer requested 2–6 Nov.', for: 'e02', read: false },
    { id: 'n5', at: '2026-10-14 08:30', text: 'Tim van Dijk’s request is ready for final approval.', for: 'e13', read: false },
    { id: 'n6', at: '2026-10-19 07:40', text: 'Femke Smit reported sick (registered).', for: 'e14', read: false },
  ] };
}
