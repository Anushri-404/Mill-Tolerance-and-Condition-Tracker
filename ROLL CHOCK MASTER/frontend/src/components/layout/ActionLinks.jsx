import './ActionLinks.css'

function ActionLinks({ actions = [] }) {
  return (
    <div className="action-links">
      {actions.map((action, idx) => (
        <span key={action.label}>
          <a
            className="action-links__link"
            href="#"
            onClick={(e) => {
              e.preventDefault()
              action.onClick?.()
            }}
          >
            {action.label}
          </a>
          {idx < actions.length - 1 && <span className="action-links__sep"> | </span>}
        </span>
      ))}
    </div>
  )
}

export default ActionLinks
