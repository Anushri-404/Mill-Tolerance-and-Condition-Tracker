import PageHeader from '../components/layout/PageHeader'
import ActionLinks from '../components/layout/ActionLinks'
import LogObservationForm from '../components/form/LogObservationForm'
import './LogObservationPage.css'

function LogObservationPage() {
  const actions = [
    { label: 'Save', onClick: () => console.log('Save clicked') },
    { label: 'Refresh', onClick: () => console.log('Refresh clicked') },
    { label: 'Cancel', onClick: () => console.log('Cancel clicked') },
  ]

  return (
    <main className="log-page">
      <PageHeader title="Strip Path Management - Log Observation" />
      <ActionLinks actions={actions} />
      <section className="log-page__panel">
        <LogObservationForm />
      </section>
    </main>
  )
}

export default LogObservationPage
