import { useState } from 'react'
import LogObservationPage from './pages/LogObservationPage'
import ReportPage from './pages/ReportPage'
import PageTabs from './components/layout/PageTabs'
import RollChockPage from "./RollChockMaster/RollChockPage";
import './App.css'

function App() {
  const [activeTab, setActiveTab] = useState('log')

  return (
    <div className="app">
      <PageTabs active={activeTab} onChange={setActiveTab} />
      <div className="app__body">
        {activeTab === 'log' && <LogObservationPage />}
        {activeTab === 'report' && <ReportPage />}
        {activeTab === 'chock' && <RollChockPage />}
      </div>
    </div>
  )
}

export default App