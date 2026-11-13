import { Avatar, Icon, PageAction, Status } from './shared'

const doctors = [
  { initials: 'AM', name: 'Alex Morgan', specialty: 'General Practice', email: 'alex.morgan@cliniq.test', patients: '248 patients', hours: '9:00 AM – 5:00 PM', tone: 'lavender', status: 'Available' },
  { initials: 'WC', name: 'Wei Chen', specialty: 'Family Medicine', email: 'wei.chen@cliniq.test', patients: '196 patients', hours: '8:30 AM – 4:30 PM', tone: 'blue', status: 'In consultation' },
  { initials: 'ER', name: 'Elena Rivera', specialty: 'Cardiology', email: 'elena.rivera@cliniq.test', patients: '174 patients', hours: '10:00 AM – 6:00 PM', tone: 'peach', status: 'Available' },
  { initials: 'JK', name: 'Jordan Kim', specialty: 'Pediatrics', email: 'jordan.kim@cliniq.test', patients: '213 patients', hours: '9:00 AM – 5:00 PM', tone: 'mint', status: 'Away' },
  { initials: 'PS', name: 'Priya Shah', specialty: 'Dermatology', email: 'priya.shah@cliniq.test', patients: '182 patients', hours: '8:00 AM – 4:00 PM', tone: 'pink', status: 'Available' },
  { initials: 'DM', name: 'Daniel Moore', specialty: 'Orthopedics', email: 'daniel.moore@cliniq.test', patients: '167 patients', hours: '9:30 AM – 5:30 PM', tone: 'sand', status: 'Available' },
]

export default function DoctorsPage() {
  return (
    <>
      <div className="page-toolbar"><div className="filter-tabs"><span className="filter-tab active">All doctors <i>6</i></span><span className="filter-tab">Available today <i>4</i></span></div><PageAction>Add doctor</PageAction></div>
      <div className="doctor-grid">
        {doctors.map((doctor) => <article className="doctor-card" key={doctor.name}>
          <div className="doctor-card-top"><Avatar initials={doctor.initials} className={`avatar-large avatar-${doctor.tone}`} /><span className="doctor-more"><Icon name="more" size={19} /></span></div>
          <h2>{doctor.name}</h2><span className="doctor-specialty">{doctor.specialty}</span><span className="doctor-email">{doctor.email}</span>
          <div className="doctor-card-line" />
          <div className="doctor-meta"><span><Icon name="users" size={16} />{doctor.patients}</span><span><Icon name="clock" size={16} />{doctor.hours}</span></div>
          <div className="doctor-card-bottom"><Status tone={doctor.status === 'Available' ? 'green' : doctor.status === 'Away' ? 'gray' : 'blue'}>{doctor.status}</Status><span className="doctor-view">View profile <Icon name="arrow" size={14} /></span></div>
        </article>)}
      </div>
    </>
  )
}
