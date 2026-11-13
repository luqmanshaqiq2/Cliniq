import { Avatar, DataTable, Icon, PageAction, SearchField, Status } from './shared'

const members = [
  ['AM', 'Alex Morgan', 'alex.morgan@cliniq.test', 'Administrator', 'Active', 'lavender'],
  ['WC', 'Wei Chen', 'wei.chen@cliniq.test', 'Doctor', 'Active', 'blue'],
  ['ER', 'Elena Rivera', 'elena.rivera@cliniq.test', 'Doctor', 'Active', 'peach'],
  ['JK', 'Jordan Kim', 'jordan.kim@cliniq.test', 'Doctor', 'Active', 'mint'],
  ['SM', 'Sage Miller', 'sage.miller@cliniq.test', 'Receptionist', 'Active', 'pink'],
  ['RH', 'Riley Hayes', 'riley.hayes@cliniq.test', 'Billing staff', 'Invited', 'sand'],
]

export default function TeamPage() {
  return (
    <>
      <div className="team-banner"><span className="team-banner-icon"><Icon name="users" size={20} /></span><div><strong>Your clinic team</strong><span>Manage the people who make great care possible.</span></div><span className="team-total"><b>6</b> team members</span></div>
      <div className="page-toolbar"><div className="filter-tabs"><span className="filter-tab active">All members <i>6</i></span><span className="filter-tab">Active <i>5</i></span><span className="filter-tab">Invited <i>1</i></span></div><PageAction>Invite team member</PageAction></div>
      <div className="panel list-panel">
        <div className="list-toolbar"><SearchField label="Search team..." /><span className="filter-select">All roles<Icon name="chevron" size={14} /></span></div>
        <DataTable headers={['TEAM MEMBER', 'ROLE', 'STATUS', '']}>
          {members.map(([initials, name, email, role, status, color]) => <tr key={email}><td><div className="table-person"><Avatar initials={initials} className={`avatar-${color}`} /><span className="person-stack"><strong>{name}</strong><small>{email}</small></span></div></td><td><span className="role-pill">{role}</span></td><td><Status tone={status === 'Active' ? 'green' : 'orange'}>{status}</Status></td><td className="row-more"><Icon name="more" size={18} /></td></tr>)}
        </DataTable>
        <div className="table-footer"><span><strong>6</strong> team members</span><span className="team-access-note"><Icon name="check" size={15} /> Access is managed by your clinic administrator</span></div>
      </div>
    </>
  )
}
