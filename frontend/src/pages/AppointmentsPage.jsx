import { Avatar, DataTable, Icon, PageAction, SearchField, Status } from './shared'

const appointments = [
  ['09:00 AM', 'OL', 'Olivia Lawson', 'Annual check-up', 'Dr. Alex Morgan', 'Confirmed', 'lavender'],
  ['09:30 AM', 'JW', 'James Wilson', 'Follow-up visit', 'Dr. Wei Chen', 'Checked in', 'blue'],
  ['10:15 AM', 'SP', 'Sofia Patel', 'Cardiology consult', 'Dr. Alex Morgan', 'Confirmed', 'peach'],
  ['11:00 AM', 'EC', 'Ethan Carter', 'Lab results review', 'Dr. Elena Rivera', 'Confirmed', 'mint'],
  ['11:30 AM', 'MC', 'Mia Cooper', 'New patient visit', 'Dr. Wei Chen', 'Waiting', 'pink'],
  ['01:00 PM', 'NB', 'Noah Brooks', 'Preventive care', 'Dr. Alex Morgan', 'Confirmed', 'sand'],
  ['02:30 PM', 'AG', 'Ava Garcia', 'Follow-up visit', 'Dr. Elena Rivera', 'Completed', 'blue'],
]

export default function AppointmentsPage() {
  return (
    <>
      <div className="page-toolbar">
        <div className="filter-tabs"><span className="filter-tab active">All appointments <i>24</i></span><span className="filter-tab">Upcoming <i>18</i></span><span className="filter-tab">Completed</span></div>
        <PageAction>New appointment</PageAction>
      </div>
      <div className="panel list-panel">
        <div className="list-toolbar"><SearchField label="Search appointments..." /><div className="toolbar-filters"><span className="filter-select"><Icon name="calendar" size={16} />May 21, 2024<Icon name="chevron" size={14} /></span><span className="filter-select">All doctors<Icon name="chevron" size={14} /></span></div></div>
        <DataTable headers={['TIME', 'PATIENT', 'VISIT TYPE', 'DOCTOR', 'STATUS']}>
          {appointments.map(([time, initials, patient, visit, doctor, status, color]) => <tr key={time}><td className="time-cell">{time}</td><td><div className="table-person"><Avatar initials={initials} className={`avatar-${color}`} /><strong>{patient}</strong></div></td><td>{visit}</td><td>{doctor}</td><td><Status tone={status === 'Checked in' ? 'blue' : status === 'Waiting' ? 'orange' : status === 'Completed' ? 'gray' : 'green'}>{status}</Status></td></tr>)}
        </DataTable>
        <div className="table-footer"><span>Showing <strong>7</strong> of <strong>24</strong> appointments</span><div className="pagination"><span className="page-arrow"><Icon name="chevron" size={15} /></span><span className="page-number current">1</span><span className="page-number">2</span><span className="page-number">3</span><span className="page-arrow next"><Icon name="chevron" size={15} /></span></div></div>
      </div>
    </>
  )
}
