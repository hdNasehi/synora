// Synora — language (English / Persian) and calendar (Gregorian / Solar Hijri) support.
import { FA } from './fa.js';

export const LOCALE = { lang: 'en', cal: 'gregorian' };
export function setLocale(lang, cal) { LOCALE.lang = lang; LOCALE.cal = cal; }
export const isFa = () => LOCALE.lang === 'fa';
export const isSolar = () => LOCALE.cal === 'solar';

const FA_DIGITS = '۰۱۲۳۴۵۶۷۸۹';
export const faDigits = (s) => String(s).replace(/[0-9]/g, (d) => FA_DIGITS[+d]).replace(/(\d|[۰-۹])\.(?=[۰-۹])/g, '$1٫');
export const localDigits = (s) => (isFa() ? faDigits(s) : String(s));

// t('Waiting for {name}', { name }) — English text is the key; Persian comes from FA.
export function t(s, vars) {
  if (s == null) return s;
  let out = isFa() && FA[s] !== undefined ? FA[s] : s;
  if (vars) out = out.replace(/\{(\w+)\}/g, (m, k) => (vars[k] !== undefined ? vars[k] : m));
  return out;
}
// Plural helper for "1 day" / "3 days".
export const tn = (n, one, many) => t(n === 1 ? one : many, { n: num(n) });
export const num = (n) => {
  const s = (Math.round(n * 100) / 100).toLocaleString('en-GB', { minimumFractionDigits: n % 1 ? 1 : 0, maximumFractionDigits: 2 });
  return s;
};

// ---- calendars -------------------------------------------------------------------------
const toDate = (iso) => new Date(iso + 'T00:00:00Z');
const addDays = (iso, n) => new Date(toDate(iso).getTime() + n * 86400000).toISOString().slice(0, 10);
let persianFmt;
const cache = new Map();
export function toJalali(iso) {
  if (cache.has(iso)) return cache.get(iso);
  persianFmt = persianFmt || new Intl.DateTimeFormat('en-u-ca-persian-nu-latn', { year: 'numeric', month: 'numeric', day: 'numeric', timeZone: 'UTC' });
  const parts = persianFmt.formatToParts(toDate(iso));
  const get = (type) => +parts.find((p) => p.type === type).value.replace(/[^0-9]/g, '');
  const r = { y: get('year'), m: get('month'), d: get('day') };
  cache.set(iso, r);
  return r;
}

const WD_EN = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
const WD_EN_LONG = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
const WD_FA = ['یکشنبه', 'دوشنبه', 'سه‌شنبه', 'چهارشنبه', 'پنجشنبه', 'جمعه', 'شنبه'];
const WD_FA_MIN = ['ی', 'د', 'س', 'چ', 'پ', 'ج', 'ش'];
const WD_EN_MIN = ['S', 'M', 'T', 'W', 'T', 'F', 'S'];
const WD_EN_2 = ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'];
const GM_EN = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
const GM_EN_LONG = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
const GM_FA = ['ژانویه', 'فوریه', 'مارس', 'آوریل', 'مه', 'ژوئن', 'ژوئیه', 'اوت', 'سپتامبر', 'اکتبر', 'نوامبر', 'دسامبر'];
const JM_EN = ['Farvardin', 'Ordibehesht', 'Khordad', 'Tir', 'Mordad', 'Shahrivar', 'Mehr', 'Aban', 'Azar', 'Dey', 'Bahman', 'Esfand'];
const JM_FA = ['فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور', 'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'];

export const weekdayShort = (d) => (isFa() ? WD_FA[d] : WD_EN[d]);
export const weekdayLong = (d) => (isFa() ? WD_FA[d] : WD_EN_LONG[d]);
export const weekdayMin = (d) => (isFa() ? WD_FA_MIN[d] : WD_EN_MIN[d]);
export const weekdayTwo = (d) => (isFa() ? WD_FA_MIN[d] : WD_EN_2[d]);
// First day of the week: Saturday for the Solar calendar or Persian UI, Monday otherwise.
export const weekStart = () => (isSolar() || isFa() ? 6 : 1);

// Calendar-aware parts of a date.
export function parts(iso) {
  const g = toDate(iso);
  if (isSolar()) {
    const j = toJalali(iso);
    return { y: j.y, m: j.m, d: j.d, month: (isFa() ? JM_FA : JM_EN)[j.m - 1], monthLong: (isFa() ? JM_FA : JM_EN)[j.m - 1] };
  }
  const m = g.getUTCMonth();
  return { y: g.getUTCFullYear(), m: m + 1, d: g.getUTCDate(), month: (isFa() ? GM_FA : GM_EN)[m], monthLong: (isFa() ? GM_FA : GM_EN_LONG)[m] };
}
// Month containing iso, in the active calendar: first day, list of ISO days, title.
export function monthOf(iso) {
  const p = parts(iso);
  let first = addDays(iso, -(p.d - 1));
  const days = [];
  for (let d = first; parts(d).m === p.m; d = addDays(d, 1)) days.push(d);
  return { first, days, title: localDigits(`${p.monthLong} ${p.y}`) };
}
export function shiftMonth(iso, n) {
  const m = monthOf(iso);
  if (n > 0) return addDays(m.days[m.days.length - 1], 1);
  if (n < 0) return monthOf(addDays(m.first, -1)).first;
  return m.first;
}

// "Thu 12 Nov 2026" · "پنجشنبه ۲۲ آبان ۱۴۰۵" · "Thu 22 Aban 1405"
export function fmt(iso, opts = {}) {
  if (!iso) return '—';
  const p = parts(iso);
  const out = [];
  if (opts.weekday !== false) out.push(weekdayShort(toDate(iso).getUTCDay()));
  out.push(p.d, p.month);
  if (opts.year !== false) out.push(p.y);
  return localDigits(out.join(' '));
}
export function fmtRange(a, b, opts = {}) {
  if (!b || a === b) return fmt(a, opts);
  const sameYear = parts(a).y === parts(b).y;
  return fmt(a, { ...opts, year: sameYear ? false : opts.year }) + ' – ' + fmt(b, opts);
}
export const fmtLong = (iso) => {
  const p = parts(iso);
  return localDigits(`${weekdayLong(toDate(iso).getUTCDay())} ${p.d} ${p.monthLong} ${p.y}`);
};
export const fmtStamp = (s) => {
  if (!s) return '';
  const [d, tm] = s.split(' ');
  return fmt(d, { weekday: false }) + (tm ? (isFa() ? '، ' : ', ') + localDigits(tm) : '');
};
export const calendarName = () => (isSolar() ? t('Solar Hijri') : t('Gregorian'));
