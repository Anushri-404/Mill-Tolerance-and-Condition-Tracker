import { useState } from 'react'
import LogObservationPage from './pages/LogObservationPage'
import ReportPage from './pages/ReportPage'
import PageTabs from './components/layout/PageTabs'
import './App.css'

function App() {
  const [activeTab, setActiveTab] = useState('log')

  return (
    <div className="app">
      <PageTabs active={activeTab} onChange={setActiveTab} />
      <div className="app__body">
        {activeTab === 'log' ? <LogObservationPage /> : <ReportPage />}
      </div>
    </div>
  )
}

export default App