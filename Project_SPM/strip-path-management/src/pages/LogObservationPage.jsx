import { useRef } from 'react'
import PageHeader from '../components/layout/PageHeader'
import ActionLinks from '../components/layout/ActionLinks'
import LogObservationForm from '../components/form/LogObservationForm'
import './LogObservationPage.css'

function LogObservationPage() {
  const formRef = useRef(null)

  const actions = [
    { label: 'Save', onClick: () => formRef.current?.save() },
    { label: 'Refresh', onClick: () => formRef.current?.refresh() },
    { label: 'Cancel', onClick: () => formRef.current?.cancel() },
  ]

  return (
    <main className="log-page">
      <PageHeader title="Strip Path Management - Log Observation" />
      <ActionLinks actions={actions} />
      <section className="log-page__panel">
        <LogObservationForm ref={formRef} />
      </section>
    </main>
  )
}

export default LogObservationPage
