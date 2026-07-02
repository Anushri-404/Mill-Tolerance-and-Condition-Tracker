import { useState } from 'react'
import FormField from '../common/FormField'
import TextInput from '../common/TextInput'
import SelectField from '../common/SelectField'
import DateInput from '../common/DateInput'
import FileInput from '../common/FileInput'
import TextArea from '../common/TextArea'
import {
  SECTION_OPTIONS,
  EQUIPMENT_LEVEL1_OPTIONS,
  EQUIPMENT_LEVEL2_OPTIONS,
  OBSERVATION_TYPE_OPTIONS,
  AFFECTED_PORTION_OPTIONS,
  SEVERITY_OPTIONS,
} from '../../data/formOptions'
import './LogObservationForm.css'

const INITIAL_STATE = {
  section: 'EXIT SECTION',
  equipmentLevel1: 'Side Guide Plate',
  equipmentLevel2: 'Plate',
  diameter: 'NA',
  hardness: '40',
  rollCoating: 'Mild Steel',
  maintenancePhilosophy: '6BM/TBM',
  replacementFrequency: '2 weeks',
  touchPoint: 'Bottom',
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
function LogObservationForm() {
  const [form, setForm] = useState(INITIAL_STATE)

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
            options={SECTION_OPTIONS}
          />
        </FormField>
        <FormField label="Equipment Level1" required>
          <SelectField
            value={form.equipmentLevel1}
            onChange={handleChange('equipmentLevel1')}
            options={EQUIPMENT_LEVEL1_OPTIONS}
          />
        </FormField>
        <FormField label="Equipment Level2" required>
          <SelectField
            value={form.equipmentLevel2}
            onChange={handleChange('equipmentLevel2')}
            options={EQUIPMENT_LEVEL2_OPTIONS}
          />
        </FormField>
        <div className="log-form__filler" aria-hidden="true" />

        {/* Row 2 */}
        <FormField label="Diameter">
          <TextInput value={form.diameter} onChange={handleChange('diameter')} variant="muted" />
        </FormField>
        <FormField label="Hardness">
          <TextInput value={form.hardness} onChange={handleChange('hardness')} variant="muted" />
        </FormField>
        <div className="log-form__filler" aria-hidden="true" />
        <div className="log-form__filler" aria-hidden="true" />

        {/* Row 3 */}
        <FormField label="Roll Coating">
          <TextInput value={form.rollCoating} onChange={handleChange('rollCoating')} variant="muted" />
        </FormField>
        <FormField label="Maintenance Philosophy">
          <TextInput
            value={form.maintenancePhilosophy}
            onChange={handleChange('maintenancePhilosophy')}
            variant="muted"
          />
        </FormField>
        <FormField label="Replacement Frequency">
          <TextInput
            value={form.replacementFrequency}
            onChange={handleChange('replacementFrequency')}
            variant="muted"
          />
        </FormField>
        <FormField label="Touch Point">
          <TextInput value={form.touchPoint} onChange={handleChange('touchPoint')} variant="muted" />
        </FormField>

        {/* Row 4 */}
        <FormField label="Observation Type" required>
          <SelectField
            value={form.observationType}
            onChange={handleChange('observationType')}
            options={OBSERVATION_TYPE_OPTIONS}
          />
        </FormField>
        <FormField label="Affected Portion" required>
          <SelectField
            value={form.affectedPortion}
            onChange={handleChange('affectedPortion')}
            options={AFFECTED_PORTION_OPTIONS}
          />
        </FormField>
        <FormField label="Severity" required>
          <SelectField
            value={form.severity}
            onChange={handleChange('severity')}
            options={SEVERITY_OPTIONS}
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
          <FileInput onChange={handleFileChange} />
        </FormField>
        <div className="log-form__filler" aria-hidden="true" />
      </div>
    </form>
  )
}

export default LogObservationForm
