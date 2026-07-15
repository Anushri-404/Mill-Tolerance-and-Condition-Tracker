import './PageTabs.css'

function PageTabs({ active, onChange }) {
  return (
    <div className="page-tabs">
      <button
        type="button"
        className={'page-tabs__tab' + (active === 'log' ? ' page-tabs__tab--active' : '')}
        onClick={() => onChange('log')}
      >
        Log
      </button>
      <button
        type="button"
        className={'page-tabs__tab' + (active === 'report' ? ' page-tabs__tab--active' : '')}
        onClick={() => onChange('report')}
      >
        Report
      </button>
    </div>
  )
}

export default PageTabs