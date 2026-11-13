import { Avatar, DataTable, Icon, PageAction, SearchField } from './shared'

const patients = [
  ['OL', 'Olivia Lawson', 'CL-20481', 'olivia.lawson@email.com', '(555) 201-4821', 'May 21, 2024', 'lavender'],
  ['JW', 'James Wilson', 'CL-20480', 'j.wilson@email.com', '(555) 301-7184', 'May 21, 2024', 'blue'],
  ['SP', 'Sofia Patel', 'CL-20479', 'sofia.patel@email.com', '(555) 410-8820', 'May 20, 2024', 'peach'],
  ['EC', 'Ethan Carter', 'CL-20478', 'ethan.carter@email.com', '(555) 209-1765', 'May 20, 2024', 'mint'],
  ['MC', 'Mia Cooper', 'CL-20477', 'mia.cooper@email.com', '(555) 509-3021', 'May 18, 2024', 'pink'],
  ['NB', 'Noah Brooks', 'CL-20476', 'noah.brooks@email.com', '(555) 618-4502', 'May 17, 2024', 'sand'],
  ['AG', 'Ava Garcia', 'CL-20475', 'ava.garcia@email.com', '(555) 702-1675', 'May 16, 2024', 'blue'],
]

export default function PatientsPage() {
  return (
    <>
      <div className="patient-summary">
        <div><span className="summary-label">TOTAL PATIENTS</span><strong>1,284</strong><small><b>+38</b> added this month</small></div>
        <span className="summary-divider" />
        <div><span className="summary-label">NEW THIS WEEK</span><strong>12</strong><small>Across all providers</small></div>
        <span className="summary-divider" />
        <div><span className="summary-label">ACTIVE PATIENTS</span><strong>1,106</strong><small>86% of patient base</small></div>
      </div>
      <div className="page-toolbar patient-toolbar"><div className="filter-tabs"><span className="filter-tab active">All patients <i>1,284</i></span><span className="filter-tab">Recently added</span><span className="filter-tab">Archived</span></div><PageAction>New patient</PageAction></div>
      <div className="panel list-panel">
        <div className="list-toolbar"><SearchField label="Search by name, email or ID..." /><span className="filter-select"><Icon name="more" size={17} />More filters</span></div>
        <DataTable headers={['PATIENT', 'PATIENT ID', 'CONTACT', 'PHONE', 'LAST VISIT']}>
          {patients.map(([initials, name, id, email, phone, visit, color]) => <tr key={id}><td><div className="table-person"><Avatar initials={initials} className={`avatar-${color}`} /><strong>{name}</strong></div></td><td className="muted-cell">{id}</td><td>{email}</td><td>{phone}</td><td>{visit}</td></tr>)}
        </DataTable>
        <div className="table-footer"><span>Showing <strong>7</strong> of <strong>1,284</strong> patients</span><div className="pagination"><span className="page-arrow"><Icon name="chevron" size={15} /></span><span className="page-number current">1</span><span className="page-number">2</span><span className="page-number">3</span><span className="page-number">...</span><span className="page-number">184</span><span className="page-arrow next"><Icon name="chevron" size={15} /></span></div></div>
      </div>
    </>
  )
}
