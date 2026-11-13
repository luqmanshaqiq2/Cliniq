import { Avatar, DataTable, Icon, PageAction, SearchField, Status } from './shared'

const invoices = [
  ['INV-2024-0842', 'OL', 'Olivia Lawson', '$240.00', 'May 21, 2024', 'Paid', 'lavender'],
  ['INV-2024-0841', 'JW', 'James Wilson', '$185.00', 'May 21, 2024', 'Pending', 'blue'],
  ['INV-2024-0840', 'SP', 'Sofia Patel', '$320.00', 'May 20, 2024', 'Overdue', 'peach'],
  ['INV-2024-0839', 'EC', 'Ethan Carter', '$95.00', 'May 20, 2024', 'Paid', 'mint'],
  ['INV-2024-0838', 'MC', 'Mia Cooper', '$150.00', 'May 18, 2024', 'Pending', 'pink'],
  ['INV-2024-0837', 'NB', 'Noah Brooks', '$275.00', 'May 17, 2024', 'Paid', 'sand'],
]

export default function BillingPage() {
  return (
    <>
      <div className="billing-stats">
        <article className="billing-stat"><span className="billing-stat-icon quick-mint"><Icon name="wallet" size={18} /></span><small>REVENUE THIS MONTH</small><strong>$18,420<span className="billing-trend">↗ 8.2%</span></strong><span className="billing-muted">Compared to last month</span></article>
        <article className="billing-stat"><span className="billing-stat-icon quick-lavender"><Icon name="file" size={18} /></span><small>OUTSTANDING</small><strong>$3,280</strong><span className="billing-muted">Across 12 invoices</span></article>
        <article className="billing-stat"><span className="billing-stat-icon quick-peach"><Icon name="clock" size={18} /></span><small>OVERDUE</small><strong>$640</strong><span className="billing-muted">Across 3 invoices</span></article>
      </div>
      <div className="page-toolbar"><div className="filter-tabs"><span className="filter-tab active">All invoices <i>84</i></span><span className="filter-tab">Outstanding <i>12</i></span><span className="filter-tab">Overdue <i>3</i></span><span className="filter-tab">Paid</span></div><PageAction>New invoice</PageAction></div>
      <div className="panel list-panel">
        <div className="list-toolbar"><SearchField label="Search invoices or patients..." /><span className="filter-select"><Icon name="calendar" size={16} />This month<Icon name="chevron" size={14} /></span></div>
        <DataTable headers={['INVOICE', 'PATIENT', 'AMOUNT', 'ISSUED', 'STATUS']}>
          {invoices.map(([id, initials, patient, amount, date, status, color]) => <tr key={id}><td className="time-cell">{id}</td><td><div className="table-person"><Avatar initials={initials} className={`avatar-${color}`} /><strong>{patient}</strong></div></td><td className="amount-cell">{amount}</td><td>{date}</td><td><Status tone={status === 'Paid' ? 'green' : status === 'Overdue' ? 'red' : 'orange'}>{status}</Status></td></tr>)}
        </DataTable>
        <div className="table-footer"><span>Showing <strong>6</strong> of <strong>84</strong> invoices</span><div className="pagination"><span className="page-arrow"><Icon name="chevron" size={15} /></span><span className="page-number current">1</span><span className="page-number">2</span><span className="page-number">3</span><span className="page-arrow next"><Icon name="chevron" size={15} /></span></div></div>
      </div>
    </>
  )
}
