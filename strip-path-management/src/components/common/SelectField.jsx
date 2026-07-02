import './inputs.css'

function SelectField({ value, onChange, options, placeholder = '--Select--' }) {
  return (
    <select className="field-select" value={value} onChange={onChange}>
      <option value="">{placeholder}</option>
      {options.map((option) => (
        <option key={option} value={option}>
          {option}
        </option>
      ))}
    </select>
  )
}

export default SelectField
