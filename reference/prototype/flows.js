// The seven guided prototype flows. Each step completes when the app emits its event.
export const FLOWS = [
  {
    id: 'f1', n: 1, title: 'Request annual leave', persona: 'Sanne de Vries (Employee)', role: 'employee', screen: 'dashboard',
    goal: 'Book Thu 12 – Mon 16 Nov and see it deduct 3 working days.',
    steps: [
      { evt: 'nav:request', label: 'Click “Request leave” on the dashboard', hint: 'Top-right of the greeting, or in the sidebar.' },
      { evt: 'dates:2026-11-12:2026-11-16', label: 'Select Thu 12 Nov – Mon 16 Nov 2026', hint: 'Open the date picker, click 12 Nov, then 16 Nov. Or use the “Try” shortcut.' },
      { evt: 'calc-reviewed', label: 'Review the calculation: 3 working days, Sat + Sun excluded', hint: 'Check the summary panel on the right, then tick “I’ve reviewed”.' },
      { evt: 'submitted', label: 'Submit the request', hint: 'Use the Submit request button.' },
      { evt: 'view-new-request', label: 'View the pending request and its approval route', hint: 'Choose “View request” in the confirmation.' },
    ],
  },
  {
    id: 'f2', n: 2, title: 'Supervisor approves', persona: 'Daan Bakker (Supervisor)', role: 'supervisor', screen: 'inbox',
    goal: 'Approve Joris Meijer’s request and watch it move to the manager.', autoDone: ['nav:inbox'],
    steps: [
      { evt: 'nav:inbox', label: 'Open the Approval inbox' },
      { evt: 'open:LR-2026-0420', label: 'Open Joris Meijer’s request (2–6 Nov)', hint: 'Click the row.' },
      { evt: 'reviewed:LR-2026-0420', label: 'Review balance, availability and the timeline', hint: 'Scroll the details drawer.' },
      { evt: 'approved:LR-2026-0420', label: 'Approve with an optional comment', hint: 'Approve → Confirm approval.' },
      { evt: 'status:LR-2026-0420:pending_manager', label: 'See the status change to Pending Manager Approval' },
    ],
  },
  {
    id: 'f3', n: 3, title: 'Manager gives final approval', persona: 'Eva Jansen (Manager)', role: 'manager', screen: 'final',
    goal: 'Give final approval to Ahmed El Amrani’s 14–18 Dec request.', autoDone: ['nav:final'],
    steps: [
      { evt: 'nav:final', label: 'Open Final approvals' },
      { evt: 'open:LR-2026-0399', label: 'Open Ahmed El Amrani’s request', hint: 'Note the staffing-conflict flag.' },
      { evt: 'approved:LR-2026-0399', label: 'Approve as the final step' },
      { evt: 'status:LR-2026-0399:approved', label: 'See the request become Approved and recorded' },
    ],
  },
  {
    id: 'f4', n: 4, title: 'Reject with a reason', persona: 'Daan Bakker (Supervisor)', role: 'supervisor', screen: 'inbox',
    goal: 'Reject Mila Bos’s 30 Nov – 4 Dec request; a reason is mandatory.',
    steps: [
      { evt: 'open:LR-2026-0418', label: 'Open Mila Bos’s request', hint: 'It carries a team-availability warning.' },
      { evt: 'reject-open', label: 'Click Reject' },
      { evt: 'reject-blocked', label: 'Try to confirm without a reason — it’s blocked', hint: 'Click “Reject request” with the reason empty.' },
      { evt: 'rejected:LR-2026-0418', label: 'Enter a reason and confirm' },
      { evt: 'status:LR-2026-0418:rejected', label: 'See the Rejected status and the comment in the timeline' },
    ],
  },
  {
    id: 'f5', n: 5, title: 'HR assigns supervisors', persona: 'Lotte Visser (HR admin)', role: 'hr', screen: 'employees',
    goal: 'Give Lucas Peeters two supervisors, an approval rule and a final manager.', autoDone: ['nav:employees'],
    steps: [
      { evt: 'nav:employees', label: 'Open Employees' },
      { evt: 'edit:e07', label: 'Edit Lucas Peeters', hint: 'Find him in the list, then choose Edit.' },
      { evt: 'sup2:e07', label: 'Add Priya Raman as a second supervisor' },
      { evt: 'rule:e07', label: 'Choose the approval rule (e.g. All supervisors)' },
      { evt: 'mgr:e07', label: 'Confirm the final manager and effective date' },
      { evt: 'saved:e07', label: 'Save — the change is written to the audit log' },
    ],
  },
  {
    id: 'f6', n: 6, title: 'Create a working schedule', persona: 'Lotte Visser (HR admin)', role: 'hr', screen: 'policies', params: { tab: 'schedules' },
    goal: 'Create a 4×9 schedule, assign it to Femke Smit from 1 Dec and preview the impact.',
    steps: [
      { evt: 'schedule-new', label: 'Click “New schedule”' },
      { evt: 'schedule-days', label: 'Select the working days and hours', hint: 'E.g. Mon–Thu, 08:00–17:30.' },
      { evt: 'schedule-saved', label: 'Save the schedule' },
      { evt: 'schedule-assigned', label: 'Assign it to Femke Smit from an effective date' },
      { evt: 'schedule-preview', label: 'Compare the leave cost before and after', hint: 'The preview recalculates the same sample request under both schedules.' },
    ],
  },
  {
    id: 'f7', n: 7, title: 'Configure a public holiday', persona: 'Lotte Visser (HR admin)', role: 'hr', screen: 'policies', params: { tab: 'holidays' },
    goal: 'Decide whether Liberation Day 2027 is a company rest day and preview the effect.',
    steps: [
      { evt: 'holiday-year:2027', label: 'Switch the holiday calendar to 2027' },
      { evt: 'holiday-select:h27-06', label: 'Select Liberation Day (Wed 5 May 2027)' },
      { evt: 'holiday-toggle:h27-06', label: 'Turn “company rest day” on for NL office staff' },
      { evt: 'holiday-preview', label: 'Read the preview: 3–7 May costs 3 days instead of 4' },
    ],
  },
];
