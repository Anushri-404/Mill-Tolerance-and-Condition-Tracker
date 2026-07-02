import './PageHeader.css'
function PageHeader({ title }) {
  return (
    <div className="page-header">
      <h1 className="page-header__title">{title}</h1>
    </div>
  )
}

export default PageHeader
