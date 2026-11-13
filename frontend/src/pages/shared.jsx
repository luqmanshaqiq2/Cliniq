const iconPaths = {
  grid: ['M3.5 3.5h7v7h-7z', 'M13.5 3.5h7v4h-7z', 'M13.5 10.5h7v10h-7z', 'M3.5 13.5h7v7h-7z'],
  calendar: ['M7 2.75v3.5', 'M17 2.75v3.5', 'M3.5 8.5h17', 'M5 4.75h14a1.5 1.5 0 0 1 1.5 1.5v13A1.5 1.5 0 0 1 19 20.75H5a1.5 1.5 0 0 1-1.5-1.5v-13A1.5 1.5 0 0 1 5 4.75Z', 'm8 13 2 2 5-5'],
  users: ['M16 20v-1.5a4 4 0 0 0-4-4H7a4 4 0 0 0-4 4V20', 'M9.5 10.5a4 4 0 1 0 0-8 4 4 0 0 0 0 8Z', 'M17 3.1a4 4 0 0 1 0 7.8', 'M21 20v-1.5a4 4 0 0 0-3-3.87'],
  doctor: ['M12 12a4 4 0 1 0 0-8 4 4 0 0 0 0 8Z', 'M4 21v-2a8 8 0 0 1 14.5-4.7', 'M19 14v6', 'M16 17h6'],
  file: ['M13 2.75H6a2 2 0 0 0-2 2v14.5a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V9.75Z', 'M13 2.75v7h7', 'M8 14h8', 'M8 17.5h8'],
  wallet: ['M4.5 5.25h14a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2h-13a2 2 0 0 1-2-2V5.25Z', 'M4.5 5.25v-1a2 2 0 0 1 2-2h12', 'M20.5 11h-5a2 2 0 0 0 0 4h5', 'M16 13h.01'],
  settings: ['M12 8.5a3.5 3.5 0 1 0 0 7 3.5 3.5 0 0 0 0-7Z', 'm19.4 15 .1.1a1.8 1.8 0 1 1-2.5 2.5l-.1-.1a1.8 1.8 0 0 0-3 .9v.3a1.8 1.8 0 1 1-3.6 0v-.2a1.8 1.8 0 0 0-3.1-1l-.1.1a1.8 1.8 0 1 1-2.5-2.5l.1-.1a1.8 1.8 0 0 0-.9-3h-.3a1.8 1.8 0 1 1 0-3.6h.2a1.8 1.8 0 0 0 1-3.1l-.1-.1a1.8 1.8 0 1 1 2.5-2.5l.1.1a1.8 1.8 0 0 0 3-.9v-.3a1.8 1.8 0 1 1 3.6 0v.2a1.8 1.8 0 0 0 3.1 1l.1-.1a1.8 1.8 0 1 1 2.5 2.5l-.1.1a1.8 1.8 0 0 0 .9 3h.3a1.8 1.8 0 1 1 0 3.6h-.2a1.8 1.8 0 0 0-1 3.1Z'],
  bell: ['M18 8a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9', 'M10 21h4'],
  plus: ['M12 5v14', 'M5 12h14'],
  sparkle: ['m12 3 1.9 5.8L20 11l-6.1 2.2L12 19l-1.9-5.8L4 11l6.1-2.2L12 3Z', 'm19 14 1 2.5 2.5 1-2.5 1L19 21l-1-2.5-2.5-1 2.5-1 1-2.5Z'],
  more: ['M5 12h.01', 'M12 12h.01', 'M19 12h.01'],
  chevron: ['m9 18 6-6-6-6'],
  search: ['m20 20-4.5-4.5', 'M17 10a7 7 0 1 1-14 0 7 7 0 0 1 14 0Z'],
  arrow: ['M5 12h14', 'm13 6 6 6-6 6'],
  clock: ['M12 8v4l2.5 1.5', 'M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z'],
  check: ['m5 12 4 4L19 6'],
  pin: ['M20 10c0 5-8 12-8 12S4 15 4 10a8 8 0 1 1 16 0Z', 'M12 10a2.5 2.5 0 1 0 0 5 2.5 2.5 0 0 0 0-5Z'],
}

export function Icon({ name, size = 18, strokeWidth = 1.7 }) {
  return (
    <svg aria-hidden="true" width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={strokeWidth} strokeLinecap="round" strokeLinejoin="round">
      {(iconPaths[name] || iconPaths.grid).map((path) => <path d={path} key={path} />)}
    </svg>
  )
}

export function Avatar({ initials, className = '' }) {
  return <span className={`avatar ${className}`}>{initials}</span>
}

export function Status({ children, tone = 'green' }) {
  return <span className={`status status-${tone}`}><i />{children}</span>
}

export function SectionHeading({ title, detail, action, onAction }) {
  return (
    <div className="section-heading">
      <div><h2>{title}</h2>{detail && <p>{detail}</p>}</div>
      {action && <button className="text-action" onClick={onAction}>{action}<Icon name="arrow" size={15} /></button>}
    </div>
  )
}

export function SearchField({ label = 'Search' }) {
  return <div className="search-field"><Icon name="search" size={17} /><span>{label}</span><kbd>⌘ K</kbd></div>
}

export function PageAction({ children }) {
  return <span className="button-static"><Icon name="plus" size={16} />{children}</span>
}

export function DataTable({ headers, children }) {
  return <div className="table-scroll"><table><thead><tr>{headers.map((header) => <th key={header}>{header}</th>)}</tr></thead><tbody>{children}</tbody></table></div>
}
