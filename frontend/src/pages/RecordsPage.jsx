import { Avatar, DataTable, Icon, SearchField, Status } from './shared'

const records = [
  ['MR-9842', 'OL', 'Olivia Lawson', 'Annual physical', 'Dr. Alex Morgan', 'May 21, 2024', 'lavender', 'Signed'],
  ['MR-9841', 'JW', 'James Wilson', 'Follow-up notes', 'Dr. Wei Chen', 'May 21, 2024', 'blue', 'Signed'],
  ['MR-9840', 'SP', 'Sofia Patel', 'Cardiology consultation', 'Dr. Alex Morgan', 'May 20, 2024', 'peach', 'In review'],
  ['MR-9839', 'EC', 'Ethan Carter', 'Lab results', 'Dr. Elena Rivera', 'May 20, 2024', 'mint', 'Signed'],
  ['MR-9838', 'MC', 'Mia Cooper', 'New patient intake', 'Dr. Wei Chen', 'May 18, 2024', 'pink', 'Signed'],
  ['MR-9837', 'NB', 'Noah Brooks', 'Preventive care', 'Dr. Alex Morgan', 'May 17, 2024', 'sand', 'In review'],
]

export default function RecordsPage() {
  return (
    <>
      <div className="record-stats">
        <div className="record-stat"><span className="quick-icon quick-blue"><Icon name="file" size={18} /></span><span><small>TOTAL RECORDS</small><strong>3,642</strong></span></div>
        <div className="record-stat"><span className="quick-icon quick-mint"><Icon name="check" size={18} /></span><span><small>COMPLETED THIS MONTH</small><strong>184</strong></span></div>
        <div className="record-stat"><span className="quick-icon quick-lavender"><Icon name="clock" size={18} /></span><span><small>NEEDS REVIEW</small><strong>6</strong></span></div>
      </div>
      <div className="page-toolbar"><div className="filter-tabs"><span className="filter-tab active">All records <i>3,642</i></span><span className="filter-tab">Needs review <i>6</i></span><span className="filter-tab">Signed</span></div><span className="button-static"><Icon name="plus" size={16} />New record</span></div>
      <div className="panel list-panel">
        <div className="list-toolbar"><SearchField label="Search records or patients..." /><span className="filter-select"><Icon name="calendar" size={16} />All dates<Icon name="chevron" size={14} /></span></div>
        <DataTable headers={['RECORD ID', 'PATIENT', 'RECORD TYPE', 'PROVIDER', 'DATE', 'STATUS']}>
          {records.map(([id, initials, name, type, doctor, date, color, status]) => <tr key={id}><td className="time-cell">{id}</td><td><div className="table-person"><Avatar initials={initials} className={`avatar-${color}`} /><strong>{name}</strong></div></td><td>{type}</td><td>{doctor}</td><td>{date}</td><td><Status tone={status === 'Signed' ? 'green' : 'orange'}>{status}</Status></td></tr>)}
        </DataTable>
        <div className="table-footer"><span>Showing <strong>6</strong> of <strong>3,642</strong> records</span><div className="pagination"><span className="page-arrow"><Icon name="chevron" size={15} /></span><span className="page-number current">1</span><span className="page-number">2</span><span className="page-number">3</span><span className="page-arrow next"><Icon name="chevron" size={15} /></span></div></div>
      </div>
    </>
  )
}
