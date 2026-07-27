import { useRef, useState } from 'react'
import PageHeader from '../components/layout/PageHeader'
import ActionLinks from '../components/layout/ActionLinks'
import ReportForm from '../components/report/ReportForm'
import ReportTable from '../components/report/ReportTable'
import './ReportPage.css'

function ReportPage() {
  const formRef = useRef(null)
  const [rows, setRows] = useState([])

  const actions = [
    { label: 'View', onClick: () => formRef.current?.view() },
    { label: 'Export', onClick: () => formRef.current?.exportData() },
    { label: 'Refresh', onClick: () => formRef.current?.refresh() },
  ]

  return (
    <main className="report-page">
      <PageHeader title="Strip Path Management - Report" />
      <ActionLinks actions={actions} />
      <section className="report-page__panel">
        <ReportForm ref={formRef} onResult={setRows} />
        <ReportTable rows={rows} />
      </section>
    </main>
  )
}

export default ReportPage