import { useState, useEffect, forwardRef, useImperativeHandle } from 'react'
import alertify from 'alertifyjs'
import 'alertifyjs/build/css/alertify.css'
import FormField from '../common/FormField'
import TextInput from '../common/TextInput'
import SelectField from '../common/SelectField'
import DateInput from '../common/DateInput'
import FileInput from '../common/FileInput'
import TextArea from '../common/TextArea'
import './LogObservationForm.css'

const MANDATORY_FIELDS = [
  { key: 'section', label: 'Section' },
  { key: 'equipmentLevel1', label: 'Equipment Level1' },
  { key: 'equipmentLevel2', label: 'Equipment Level2' },
  { key: 'observationType', label: 'Observation Type' },
  { key: 'affectedPortion', label: 'Affected Portion' },
  { key: 'severity', label: 'Severity' },
  { key: 'defectDetails', label: 'Defect Details' },
]

const INITIAL_STATE = {
  section: '',
  equipmentLevel1: '',
  equipmentLevel2: '',
  diameter: '',
  hardness: '',
  rollCoating: '',
  maintenancePhilosophy: '',
  replacementFrequency: '',
  touchPoint: '',
  observationType: '',
  affectedPortion: '',
  severity: '',
  diameterNew: '',
  hardnessNew: '',
  liningCondition: '',
  bearingCondition: '',
  bakeliteGuidePlateCondition: '',
  stripPathAuditDate: '',
  lastRollChangeDate: '',
  lastBearingGreasingDate: '',
  defectDetails: '',
  attachment: null,
}

const LogObservationForm = forwardRef((props, ref) => {
  const [form, setForm] = useState(INITIAL_STATE)
  const [sections, setSections] = useState([])
  const [equipL1List, setEquipL1List] = useState([])
  const [equipL2List, setEquipL2List] = useState([])
  const [obsTypes, setObsTypes] = useState([])
  const [affectedPortions, setAffectedPortions] = useState([])
  const [fileKey, setFileKey] = useState(0)
  const [loading, setLoading] = useState(false)

  const API_BASE = 'http://localhost:5103/api/spm'

  useEffect(() => {
    const fetchInitialData = async () => {
      try {
        const sectionsRes = await fetch(`${API_BASE}/sections`)
        if (sectionsRes.ok) {
          const data = await sectionsRes.json()
          setSections(data)
        }

        const obsTypesRes = await fetch(`${API_BASE}/observation-types`)
        if (obsTypesRes.ok) {
          const data = await obsTypesRes.json()
          setObsTypes(data)
        }

        const affectedPortionsRes = await fetch(`${API_BASE}/affected-portions`)
        if (affectedPortionsRes.ok) {
          const data = await affectedPortionsRes.json()
          setAffectedPortions(data)
        }
      } catch (err) {
        console.error('Error fetching initial master data:', err)
      }
    }

    fetchInitialData()
  }, [])

  useEffect(() => {
    if (!form.section) {
      setEquipL1List([])
      setEquipL2List([])
      setForm((prev) => ({
        ...prev,
        equipmentLevel1: '',
        equipmentLevel2: '',
      }))
      return
    }

    const fetchEquipL1 = async () => {
      try {
        const res = await fetch(`${API_BASE}/equip-l1?section=${encodeURIComponent(form.section)}`)
        if (res.ok) {
          const data = await res.json()
          setEquipL1List(data)
        }
      } catch (err) {
        console.error('Error fetching Equipment Level 1:', err)
      }
    }

    fetchEquipL1()
    // Reset selections below
    setForm((prev) => ({
      ...prev,
      equipmentLevel1: '',
      equipmentLevel2: '',
    }))
  }, [form.section])

  // Cascade 2: Section & L1 -> L2
  useEffect(() => {
    if (!form.section || !form.equipmentLevel1) {
      setEquipL2List([])
      setForm((prev) => ({
        ...prev,
        equipmentLevel2: '',
      }))
      return
    }

    const fetchEquipL2 = async () => {
      try {
        const res = await fetch(
          `${API_BASE}/equip-l2?section=${encodeURIComponent(form.section)}&equipL1=${encodeURIComponent(
            form.equipmentLevel1
          )}`
        )
        if (res.ok) {
          const data = await res.json()
          setEquipL2List(data)
        }
      } catch (err) {
        console.error('Error fetching Equipment Level 2:', err)
      }
    }

    fetchEquipL2()
   
    setForm((prev) => ({
      ...prev,
      equipmentLevel2: '',
    }))
  }, [form.section, form.equipmentLevel1])

  //  Grey Parts
  useEffect(() => {
    if (!form.equipmentLevel2) {
      setForm((prev) => ({
        ...prev,
        diameter: '',
        hardness: '',
        rollCoating: '',
        maintenancePhilosophy: '',
        replacementFrequency: '',
        touchPoint: '',
      }))
      return
    }

    const fetchGreyParts = async () => {
      try {
        const res = await fetch(`${API_BASE}/grey-parts?equipL2Id=${encodeURIComponent(form.equipmentLevel2)}`)
        if (res.ok) {
          const data = await res.json()
          setForm((prev) => ({
            ...prev,
            diameter: data.equipL2RollDia || 'NA',
            hardness: data.hardness || 'NA',
            rollCoating: data.equipL2RollCoat || 'NA',
            maintenancePhilosophy: data.maintPhilosophy || 'NA',
            replacementFrequency: data.replacementFreq || 'NA',
            touchPoint: data.equipL2TouchPoint || 'NA',
          }))
        }
      } catch (err) {
        console.error('Error fetching grey parts specs:', err)
      }
    }

    fetchGreyParts()
  }, [form.equipmentLevel2])

  const cancelForm = () => {
    setForm(INITIAL_STATE)
    setFileKey((prev) => prev + 1)
  }

  const refreshForm = async () => {
    cancelForm()
    try {
      const sectionsRes = await fetch(`${API_BASE}/sections`)
      if (sectionsRes.ok) {
        const data = await sectionsRes.json()
        setSections(data)
      }

      const obsTypesRes = await fetch(`${API_BASE}/observation-types`)
      if (obsTypesRes.ok) {
        const data = await obsTypesRes.json()
        setObsTypes(data)
      }

      const affectedPortionsRes = await fetch(`${API_BASE}/affected-portions`)
      if (affectedPortionsRes.ok) {
        const data = await affectedPortionsRes.json()
        setAffectedPortions(data)
      }
    } catch (err) {
      console.error('Error refreshing initial data:', err)
    }
  }

  const saveForm = async () => {
    setLoading(true)

    for (const field of MANDATORY_FIELDS) {
      const value = form[field.key]
      if (!value || value === '' || value === undefined) {
        alertify.error(field.label + ' is required.')
        setLoading(false)
        return
      }
    }

    const formData = new FormData()
    formData.append('equipIdL2', form.equipmentLevel2)
    formData.append('obsType', form.observationType)
    formData.append('affectedP', form.affectedPortion)
    formData.append('defDetails', form.defectDetails)
    formData.append('diameterNew', form.diameterNew)
    formData.append('hardnessNew', form.hardnessNew)
    formData.append('liningCondNew', form.liningCondition)
    formData.append('bearingCondNew', form.bearingCondition)
    formData.append('bakelitePlateCondNew', form.bakeliteGuidePlateCondition)
    formData.append('severityStatus', form.severity)
    formData.append('sectionName', form.section)
    formData.append('equipL1Desc', form.equipmentLevel1)
    formData.append('equipL2Desc', equipL2List.find(e => e.equipIdL2 === form.equipmentLevel2)?.equipDescL2 ?? '')
    if (form.stripPathAuditDate) formData.append('spAuditDate', form.stripPathAuditDate)
    if (form.lastRollChangeDate) formData.append('lastRollChangeDate', form.lastRollChangeDate)
    if (form.lastBearingGreasingDate) formData.append('lastBearGreaseDate', form.lastBearingGreasingDate)
    if (form.attachment) {
      formData.append('attachment', form.attachment)
    }
    try {
      const response = await fetch(`${API_BASE}/save-observation`, {
        method: 'POST',
        body: formData,
      })

      if (!response.ok) {
        throw new Error('Server responded with an error status: ' + response.status)
      }

      const result = await response.json()
      if (result.success) {
        alertify.success('Observation saved successfully!')
        cancelForm()
      } else {
        alertify.error('Failed to save observation: ' + result.message)
      }
    } catch (error) {
      console.error('Error saving observation:', error)
      alertify.error('Error occurred while connecting to the backend API.')
    } finally {
      setLoading(false)
    }
  }

  // 
  useImperativeHandle(ref, () => ({
    save: saveForm,
    refresh: refreshForm,
    cancel: cancelForm,
  }))

  const handleChange = (field) => (event) => {
    setForm((prev) => ({ ...prev, [field]: event.target.value }))
  }

  const handleFileChange = (event) => {
    setForm((prev) => ({ ...prev, attachment: event.target.files?.[0] ?? null }))
  }

  return (
    <form className="log-form" onSubmit={(e) => e.preventDefault()}>
      <p className="log-form__mandatory-note">Mandatory fields*</p>

      <div className="log-form__grid">
        {/* Row 1 */}
        <FormField label="Section" required>
          <SelectField
            value={form.section}
            onChange={handleChange('section')}
            options={sections}
          />
        </FormField>
        <FormField label="Equipment Level1" required>
          <SelectField
            value={form.equipmentLevel1}
            onChange={handleChange('equipmentLevel1')}
            options={equipL1List}
          />
        </FormField>
        <FormField label="Equipment Level2" required>
          <SelectField
            value={form.equipmentLevel2}
            onChange={handleChange('equipmentLevel2')}
            options={equipL2List}
          />
        </FormField>
        <div className="log-form__filler" aria-hidden="true" />

        {/* Row 2 */}
        <FormField label="Diameter">
          <TextInput value={form.diameter} onChange={handleChange('diameter')} variant="muted" disabled />
        </FormField>
        <FormField label="Hardness">
          <TextInput value={form.hardness} onChange={handleChange('hardness')} variant="muted" disabled />
        </FormField>
        <div className="log-form__filler" aria-hidden="true" />
        <div className="log-form__filler" aria-hidden="true" />

        {/* Row 3 */}
        <FormField label="Roll Coating">
          <TextInput value={form.rollCoating} onChange={handleChange('rollCoating')} variant="muted" disabled />
        </FormField>
        <FormField label="Maintenance Philosophy">
          <TextInput
            value={form.maintenancePhilosophy}
            onChange={handleChange('maintenancePhilosophy')}
            variant="muted"
            disabled
          />
        </FormField>
        <FormField label="Replacement Frequency">
          <TextInput
            value={form.replacementFrequency}
            onChange={handleChange('replacementFrequency')}
            variant="muted"
            disabled
          />
        </FormField>
        <FormField label="Touch Point">
          <TextInput value={form.touchPoint} onChange={handleChange('touchPoint')} variant="muted" disabled />
        </FormField>

        {/* Row 4 */}
        <FormField label="Observation Type" required>
          <SelectField
            value={form.observationType}
            onChange={handleChange('observationType')}
            options={obsTypes}
          />
        </FormField>
        <FormField label="Affected Portion" required>
          <SelectField
            value={form.affectedPortion}
            onChange={handleChange('affectedPortion')}
            options={affectedPortions}
          />
        </FormField>
        <FormField label="Severity" required>
          <SelectField
            value={form.severity}
            onChange={handleChange('severity')}
            options={['OK', 'Reject', 'Need Maintenance']}
          />
        </FormField>
        <FormField label="Diameter new">
          <TextInput value={form.diameterNew} onChange={handleChange('diameterNew')} />
        </FormField>

        {/* Row 5 */}
        <FormField label="Hardness new">
          <TextInput value={form.hardnessNew} onChange={handleChange('hardnessNew')} />
        </FormField>
        <FormField label="Lining Condition">
          <TextInput
            value={form.liningCondition}
            onChange={handleChange('liningCondition')}
          />
        </FormField>
        <FormField label="Bearing Condition">
          <TextInput
            value={form.bearingCondition}
            onChange={handleChange('bearingCondition')}
          />
        </FormField>
        <FormField label="Bakelite/ Guide Plate Condition">
          <TextInput
            value={form.bakeliteGuidePlateCondition}
            onChange={handleChange('bakeliteGuidePlateCondition')}
          />
        </FormField>

        {/* Row 6 */}
        <FormField label="Strip Path Audit Date">
          <DateInput
            value={form.stripPathAuditDate}
            onChange={handleChange('stripPathAuditDate')}
          />
        </FormField>
        <FormField label="Last Roll Change Date">
          <DateInput
            value={form.lastRollChangeDate}
            onChange={handleChange('lastRollChangeDate')}
          />
        </FormField>
        <FormField label="Last Bearing Greasing Date">
          <DateInput
            value={form.lastBearingGreasingDate}
            onChange={handleChange('lastBearingGreasingDate')}
          />
        </FormField>
        <div className="log-form__filler" aria-hidden="true" />

        {/* Row 7 */}
        <FormField label="Defect Details" required className="log-form__defect-cell">
          <TextArea
            value={form.defectDetails}
            onChange={handleChange('defectDetails')}
            rows={3}
          />
        </FormField>
        <FormField label="Attachment (if any)" className="log-form__attachment-cell">
          <FileInput key={fileKey} onChange={handleFileChange} />
        </FormField>
        <div className="log-form__filler" aria-hidden="true" />
      </div>
    </form>
  )
})

LogObservationForm.displayName = 'LogObservationForm'

export default LogObservationForm
