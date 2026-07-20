import './PageHeader.css'

function PageHeader({ title }) {
  return (
    <div className="page-header">
      <span className="page-header__title">{title}</span>
    </div>
  )
}

export default PageHeader
