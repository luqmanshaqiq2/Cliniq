import { Avatar, Icon, SectionHeading, Status } from './shared'

const appointments = [
  { time: '09:00 AM', initials: 'OL', name: 'Olivia Lawson', detail: 'Annual check-up', doctor: 'Dr. Morgan', tone: 'lavender', status: 'Confirmed' },
  { time: '09:30 AM', initials: 'JW', name: 'James Wilson', detail: 'Follow-up visit', doctor: 'Dr. Chen', tone: 'blue', status: 'Checked in' },
  { time: '10:15 AM', initials: 'SP', name: 'Sofia Patel', detail: 'Cardiology consult', doctor: 'Dr. Morgan', tone: 'peach', status: 'Confirmed' },
  { time: '11:00 AM', initials: 'EC', name: 'Ethan Carter', detail: 'Lab results review', doctor: 'Dr. Rivera', tone: 'mint', status: 'Confirmed' },
]

const week = [
  { day: 'MON', date: '20', count: '12' },
  { day: 'TUE', date: '21', count: '8', active: true },
  { day: 'WED', date: '22', count: '14' },
  { day: 'THU', date: '23', count: '10' },
  { day: 'FRI', date: '24', count: '9' },
  { day: 'SAT', date: '25', count: '4' },
  { day: 'SUN', date: '26', count: '—' },
]

export default function DashboardPage({ onNavigate }) {
  return (
    <>
      <div className="stats-grid">
        <article className="stat-card">
          <div className="stat-top"><span className="stat-icon mint-icon"><Icon name="calendar" size={19} /></span><span className="stat-change">+12.5%</span></div>
          <span className="stat-label">Appointments today</span><strong className="stat-value">24</strong><span className="stat-foot">vs. 21 last Tuesday</span>
          <div className="mini-bars"><i /><i /><i /><i /><i /><i /><i /><i /><i /><i /><i className="bar-current" /></div>
        </article>
        <article className="stat-card">
          <div className="stat-top"><span className="stat-icon blue-icon"><Icon name="users" size={19} /></span><span className="stat-period">This month</span></div>
          <span className="stat-label">Total patients</span><strong className="stat-value">1,284</strong><span className="stat-foot"><b>+38</b> new patients this month</span>
          <div className="mini-bars blue-bars"><i /><i /><i /><i /><i /><i /><i /><i /><i /><i /><i className="bar-current" /></div>
        </article>
        <article className="stat-card">
          <div className="stat-top"><span className="stat-icon lavender-icon"><Icon name="wallet" size={19} /></span><span className="stat-change">+8.2%</span></div>
          <span className="stat-label">Revenue this month</span><strong className="stat-value">$18,420</strong><span className="stat-foot">vs. $17,020 last month</span>
          <div className="mini-bars violet-bars"><i /><i /><i /><i /><i /><i /><i /><i /><i /><i /><i className="bar-current" /></div>
        </article>
        <article className="stat-card stat-card-plain">
          <div className="stat-top"><span className="stat-icon peach-icon"><Icon name="clock" size={19} /></span><span className="stat-period">This week</span></div>
          <span className="stat-label">Avg. wait time</span><strong className="stat-value">12 <small>min</small></strong><span className="stat-foot"><b>↓ 3 min</b> from last week</span>
          <div className="wait-meter"><span /></div>
          <span className="meter-caption">Looking good</span>
        </article>
      </div>

      <div className="dashboard-grid">
        <section className="panel appointment-panel">
          <SectionHeading title="Today’s appointments" detail="Tuesday, May 21 · 8 scheduled" action="Full schedule" onAction={() => onNavigate('appointments')} />
          <div className="schedule-list">
            {appointments.map((appointment) => (
              <div className="schedule-row" key={appointment.time}>
                <span className="schedule-time">{appointment.time}</span>
                <Avatar initials={appointment.initials} className={`avatar-${appointment.tone}`} />
                <div className="appointment-person"><strong>{appointment.name}</strong><span>{appointment.detail}</span></div>
                <span className="appointment-doctor">{appointment.doctor}</span>
                <Status tone={appointment.status === 'Checked in' ? 'blue' : 'green'}>{appointment.status}</Status>
              </div>
            ))}
          </div>
          <button className="panel-bottom-link" onClick={() => onNavigate('appointments')}>View all 8 appointments <Icon name="arrow" size={15} /></button>
        </section>

        <section className="panel week-panel">
          <SectionHeading title="Your week" detail="May 20 – 26, 2024" />
          <div className="week-strip">{/x/zxk/
          ((day) => <div className={`week-day ${day.active ? 'selected' : ''}`} key={day.day}><span>{day.day}</span><strong>{day.date}</strong><i>{day.count}</i></div>)}</div>
          <div className="week-summary"><div className="summary-icon"><Icon name="calendar" size={18} /></div><div><strong>57 appointments</strong><span>Scheduled this week</span></div><div className="summary-trend">+6%</div></div>
          <div className="week-progress"><span /></div>
          <div className="progress-caption"><span>Weekly capacity</span><strong>72%</strong></div>
          <div className="week-note"><span className="note-dot" />Wednesday is your busiest day this week.</div>
        </section>
      </div>

      <section className="panel quick-panel">
        <SectionHeading title="Your clinic at a glance" detail="A few more things to know today" />
        <div className="quick-grid">
          <button className="quick-item" onClick={() => onNavigate('patients')}><span className="quick-icon quick-mint"><Icon name="users" size={18} /></span><span><strong>New patients</strong><small>38 added this month</small></span><Icon name="chevron" size={16} /></button>
          <button className="quick-item" onClick={() => onNavigate('billing')}><span className="quick-icon quick-lavender"><Icon name="wallet" size={18} /></span><span><strong>Pending invoices</strong><small>12 awaiting payment</small></span><Icon name="chevron" size={16} /></button>
          <button className="quick-item" onClick={() => onNavigate('doctors')}><span className="quick-icon quick-blue"><Icon name="doctor" size={18} /></span><span><strong>Care team</strong><small>6 doctors on your team</small></span><Icon name="chevron" size={16} /></button>
        </div>
      </section>
    </>
  )
}
