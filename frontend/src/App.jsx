import { useState } from 'react'
import './App.css'
import AppointmentsPage from './pages/AppointmentsPage'
import BillingPage from './pages/BillingPage'
import DashboardPage from './pages/DashboardPage'
import DoctorsPage from './pages/DoctorsPage'
import PatientsPage from './pages/PatientsPage'
import RecordsPage from './pages/RecordsPage'
import TeamPage from './pages/TeamPage'
import { Icon } from './pages/shared'

const navigation = [
  { id: 'overview', label: 'Overview', icon: 'grid' },
  { id: 'appointments', label: 'Appointments', icon: 'calendar' },
  { id: 'patients', label: 'Patients', icon: 'users' },
  { id: 'doctors', label: 'Doctors', icon: 'doctor' },
  { id: 'records', label: 'Medical records', icon: 'file' },
  { id: 'billing', label: 'Billing', icon: 'wallet' },
]

const pageDetails = {
  overview: { title: 'Good morning, Dr. Morgan', subtitle: 'Here’s what’s happening at your clinic today.' },
  appointments: { title: 'Appointments', subtitle: 'A clear view of your clinic schedule.' },
  patients: { title: 'Patients', subtitle: 'Patient directory and contact details.' },
  doctors: { title: 'Doctors', subtitle: 'Your care team and specialties.' },
  records: { title: 'Medical records', subtitle: 'Recent notes and patient visits.' },
  billing: { title: 'Billing', subtitle: 'Invoices and payment overview.' },
  team: { title: 'Team & access', subtitle: 'Clinic staff and account roles.' },
}

const pageComponents = {
  overview: DashboardPage,
  appointments: AppointmentsPage,
  patients: PatientsPage,
  doctors: DoctorsPage,
  records: RecordsPage,
  billing: BillingPage,
  team: TeamPage,
}

function App() {
  const [activePage, setActivePage] = useState('overview')
  const Page = pageComponents[activePage]
  const details = pageDetails[activePage]

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <a className="brand" href="#" onClick={(event) => { event.preventDefault(); setActivePage('overview') }}>
          <span className="brand-mark"><Icon name="plus" size={21} /></span>
          <span>cliniq<span className="brand-period">.</span></span>
        </a>

        <div className="sidebar-label">WORKSPACE</div>
        <nav className="side-nav" aria-label="Main navigation">
          {navigation.map((item) => (
            <button
              className={`nav-item ${activePage === item.id ? 'active' : ''}`}
              key={item.id}
              title={item.label}
              aria-label={item.label}
              onClick={() => setActivePage(item.id)}
              aria-current={activePage === item.id ? 'page' : undefined}
            >
              <Icon name={item.icon} size={19} />
              <span>{item.label}</span>
              {item.id === 'appointments' && <span className="nav-count">8</span>}
            </button>
          ))}
        </nav>

        <div className="sidebar-spacer" />
        <div className="sidebar-label">PREFERENCES</div>
        <button
          className={`nav-item ${activePage === 'team' ? 'active' : ''}`}
          title="Team & access"
          aria-label="Team & access"
          onClick={() => setActivePage('team')}
          aria-current={activePage === 'team' ? 'page' : undefined}
        >
          <Icon name="settings" size={19} />
          <span>Team & access</span>
        </button>
        <div className="sidebar-help">
          <span className="help-orb"><Icon name="sparkle" size={18} /></span>
          <strong>Care, made simpler.</strong>
          <span>Your clinic workspace at a glance.</span>
        </div>
        <button className="profile-button" onClick={() => setActivePage('team')}>
          <span className="avatar avatar-sidebar">AM</span>
          <span className="profile-copy"><strong>Alex Morgan</strong><small>Clinic administrator</small></span>
          <Icon name="more" size={18} />
        </button>
      </aside>

      <main className="main-area">
        <header className="topbar">
          <div className="breadcrumb"><span>Cliniq</span><Icon name="chevron" size={15} /><strong>{details.title.replace('Good morning, Dr. Morgan', 'Overview')}</strong></div>
          <div className="topbar-actions">
            <div className="today-label"><Icon name="calendar" size={16} /><span>Tuesday, May 21, 2024</span></div>
            <span className="topbar-divider" />
            <span className="notification-icon"><Icon name="bell" size={19} /><i /></span>
            <span className="avatar avatar-top">AM</span>
          </div>
        </header>

        <section className="page-content">
          <div className="page-heading">
            <div>
              <div className="eyebrow">CLINIC WORKSPACE <span className="eyebrow-dot" /> TUESDAY, MAY 21</div>
              <h1>{details.title}</h1>
              <p>{details.subtitle}</p>
            </div>
            <span className="live-status"><span /> All systems normal</span>
          </div>
          <Page onNavigate={setActivePage} />
          <footer className="page-footer"><span>Cliniq <span className="brand-period">·</span> Clinic workspace</span><span>Designed for better care</span></footer>
        </section>
      </main>
    </div>
  )
}

export default App
