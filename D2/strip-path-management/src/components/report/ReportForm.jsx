import { useState, forwardRef, useImperativeHandle, useEffect } from 'react'
import alertify from 'alertifyjs'
import * as XLSX from 'xlsx'
import FormField from '../common/FormField'
import SelectField from '../common/SelectField'
import DateInput from '../common/DateInput'
import './ReportForm.css'

const INITIAL_STATE = {
  startDate: '',
  endDate: '',
  section: '',
  equipmentLevel1: '',
  equipmentLevel2: '',
}

// Maps API field -> friendly column header for the exported spreadsheet.
// Keep this in sync with ReportTable.jsx's COLUMNS list.
const EXPORT_COLUMNS = [
  { key: 'observationId', header: 'OBSERVATION_ID' },
  { key: 'section', header: 'SECTION' },
  { key: 'equipLv1', header: 'EQUIP_LV1' },
  { key: 'equipLv2Id', header: 'EQUIP_LV2_ID' },
  { key: 'equipLv2Desc', header: 'EQUIP_LV2_DESC' },
  { key: 'observation', header: 'OBSERVATION' },
  { key: 'affectedPortion', header: 'AFFECTETED_PORTION' },
  { key: 'defectDetails', header: 'DEFECT_DETAILS' },
  { key: 'diameterActual', header: 'DIAMETER_ACTUAL' },
  { key: 'rollcoatActual', header: 'ROLLCOAT_ACTUAL' },
  { key: 'rollTouchpoint', header: 'ROLL_TOUCHPOINT' },
  { key: 'hardnessActual', header: 'HARNESS_ACTUAL' },
  { key: 'maintenancePhilosophy', header: 'MAINTENANCE_PHILOSOPHY' },
  { key: 'replacementFrequency', header: 'REPLACEMENT_FREQUENCY' },
  { key: 'diameterNew', header: 'DIAMETER_NEW' },
  { key: 'hardnessNew', header: 'HARDNESS_NEW' },
  { key: 'liningCondNew', header: 'LINING_COND_NEW' },
  { key: 'bearingCondNew', header: 'BEARING_COND_NEW' },
  { key: 'bakeliteGuideplateCond', header: 'BAKELITE_GUIDEPLATE_COND' },
  { key: 'status', header: 'STATUS' },
  { key: 'stripPathAuditDate', header: 'STRIPPATH_AUDIT_DATE' },
  { key: 'lastRollchangeDate', header: 'LAST_ROLLCHANGE_DATE' },
  { key: 'lastBearingGreasingDate', header: 'LAST_BEARING_GREASING_DATE' },
  { key: 'loggedOn', header: 'LOGGEDON' },
  { key: 'attachmentUrl', header: 'ATTACHMENT' },
]

const ReportForm = forwardRef(({ onResult }, ref) => {
  const [filters, setFilters] = useState(INITIAL_STATE)
  const [sections, setSections] = useState([])
  const [equipL1List, setEquipL1List] = useState([])
  const [equipL2List, setEquipL2List] = useState([])

  const API_BASE = 'http://localhost:5103/api/spm'

  const fetchSections = () => {
    fetch(`${API_BASE}/sections`)
      .then((res) => (res.ok ? res.json() : []))
      .then(setSections)
      .catch((err) => console.error('Error fetching sections:', err))
  }

  useEffect(() => {
    fetchSections()
  }, [])

  // Cascade: Section -> L1
  useEffect(() => {
    if (!filters.section) {
      setEquipL1List([])
      setFilters((prev) => ({ ...prev, equipmentLevel1: '', equipmentLevel2: '' }))
      return
    }
    fetch(`${API_BASE}/equip-l1?section=${encodeURIComponent(filters.section)}`)
      .then((res) => (res.ok ? res.json() : []))
      .then(setEquipL1List)
      .catch((err) => console.error('Error fetching Equipment Level 1:', err))
    setFilters((prev) => ({ ...prev, equipmentLevel1: '', equipmentLevel2: '' }))
  }, [filters.section])

  // Cascade: Section & L1 -> L2
  useEffect(() => {
    if (!filters.section || !filters.equipmentLevel1) {
      setEquipL2List([])
      setFilters((prev) => ({ ...prev, equipmentLevel2: '' }))
      return
    }
    fetch(
      `${API_BASE}/equip-l2?section=${encodeURIComponent(filters.section)}&equipL1=${encodeURIComponent(
        filters.equipmentLevel1
      )}`
    )
      .then((res) => (res.ok ? res.json() : []))
      .then(setEquipL2List)
      .catch((err) => console.error('Error fetching Equipment Level 2:', err))
    setFilters((prev) => ({ ...prev, equipmentLevel2: '' }))
  }, [filters.section, filters.equipmentLevel1])

  const handleChange = (field) => (event) => {
    setFilters((prev) => ({ ...prev, [field]: event.target.value }))
  }

  const buildParams = () => {
    const params = new URLSearchParams({
      startDate: filters.startDate,
      endDate: filters.endDate,
    })
    if (filters.section) params.append('section', filters.section)
    if (filters.equipmentLevel1) params.append('equipL1', filters.equipmentLevel1)
    if (filters.equipmentLevel2) params.append('equipL2', filters.equipmentLevel2)
    return params
  }

  const viewReport = async () => {
    if (!filters.startDate || !filters.endDate) {
      alertify.error('Start Date and End Date are required.')
      return
    }
    try {
      const res = await fetch(`${API_BASE}/report?${buildParams().toString()}`)
      if (!res.ok) throw new Error('Server responded with ' + res.status)
      const data = await res.json()
      onResult(data)
    } catch (err) {
      console.error('Error fetching report:', err)
      alertify.error('Error occurred while fetching the report.')
    }
  }

  const refreshReport = () => {
    setFilters(INITIAL_STATE)
    onResult([])
    // Re-pull master data too, in case the backend switched between
    // mock and Oracle since the page was first loaded.
    fetchSections()
    setEquipL1List([])
    setEquipL2List([])
  }

  // Client-side export: re-fetches the same report data View uses, then
  // builds an .xlsx in the browser. No backend "/report/export" route
  // needed, and it works identically whether the backend is serving
  // mock or real Oracle data.
  const exportReport = async () => {
    if (!filters.startDate || !filters.endDate) {
      alertify.error('Start Date and End Date are required.')
      return
    }
    try {
      const res = await fetch(`${API_BASE}/report?${buildParams().toString()}`)
      if (!res.ok) throw new Error('Server responded with ' + res.status)
      const data = await res.json()

      if (!data.length) {
        alertify.error('No data to export for the selected filters.')
        return
      }

      const exportRows = data.map((row) => {
        const flat = {}
        for (const col of EXPORT_COLUMNS) {
          if (col.key === 'attachmentUrl') {
            flat[col.header] = row.attachmentUrl ?? 'NA'
          } else {
            flat[col.header] = row[col.key] ?? ''
          }
        }
        return flat
      })

      const worksheet = XLSX.utils.json_to_sheet(exportRows)
      const workbook = XLSX.utils.book_new()
      XLSX.utils.book_append_sheet(workbook, worksheet, 'Report')
      XLSX.writeFile(workbook, `SPM_Report_${filters.startDate}_to_${filters.endDate}.xlsx`)
    } catch (err) {
      console.error('Error exporting report:', err)
      alertify.error('Error occurred while exporting the report.')
    }
  }

  useImperativeHandle(ref, () => ({
    view: viewReport,
    refresh: refreshReport,
    exportData: exportReport,
  }))

  return (
    <div className="report-form">
      <p className="report-form__mandatory-note">Mandatory fields*</p>
      <div className="report-form__grid">
        {/* Row 1: 2 entries */}
        <FormField label="Start Date" required>
          <DateInput value={filters.startDate} onChange={handleChange('startDate')} />
        </FormField>
        <FormField label="End Date" required>
          <DateInput value={filters.endDate} onChange={handleChange('endDate')} />
        </FormField>
        <div className="report-form__filler" aria-hidden="true" />

        {/* Row 2: 3 entries */}
        <FormField label="Section">
          <SelectField value={filters.section} onChange={handleChange('section')} options={sections} />
        </FormField>
        <FormField label="Equipment Level1">
          <SelectField
            value={filters.equipmentLevel1}
            onChange={handleChange('equipmentLevel1')}
            options={equipL1List}
          />
        </FormField>
        <FormField label="Equipment Level2">
          <SelectField
            value={filters.equipmentLevel2}
            onChange={handleChange('equipmentLevel2')}
            options={equipL2List}
          />
        </FormField>
      </div>
    </div>
  )
})

ReportForm.displayName = 'ReportForm'

export default ReportForm
