import { useRef } from 'react'
import PageHeader from './components/layout/PageHeader'
import ActionLinks from './components/layout/ActionLinks'
import ChockMasterForm from './RollChockMaster/components/ChockMasterForm'
import './theme.css'
import './App.css'

function App() {
  const formRef = useRef(null)

  const actions = [
    { label: 'Query', onClick: () => formRef.current?.query() },
    { label: 'Save', onClick: () => formRef.current?.save() },
    { label: 'Clear', onClick: () => formRef.current?.clear() },
  ]

  return (
    <div className="app">
      <div className="app__toolbar">
        <PageHeader title="Roll Chock Master - Recording New Chock" />
        <ActionLinks actions={actions} />
      </div>
      <main className="app__panel">
        <ChockMasterForm ref={formRef} />
      </main>
    </div>
  )
}

export default App
