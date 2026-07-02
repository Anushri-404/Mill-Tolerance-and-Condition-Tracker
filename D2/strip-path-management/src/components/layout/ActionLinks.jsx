//save,refresh,cancel
import './ActionLinks.css'
function ActionLinks({ actions = [] }) {
  return (
    <div className="action-links">
      {actions.map((action, index) => (
        <span key={action.label} className="action-links__group">
          {index > 0 && <span className="action-links__sep">|</span>}
          <button
            type="button"
            className="action-links__link"
            onClick={action.onClick}
          >
            {action.label}
          </button>
        </span>
      ))}
    </div>
  )
}

export default ActionLinks
