import './inputs.css'

function SelectField({ value, onChange, options = [], placeholder = '--Select--' }) {
  return (
    <select className="field-select" value={value} onChange={onChange}>
      <option value="">{placeholder}</option>
      {options.map((option) => {
        const isObject = typeof option === 'object' && option !== null
        const val = isObject ? (option.value ?? option.codeId ?? option.equipIdL2 ?? option) : option
        const label = isObject ? (option.label ?? option.codeDesc ?? option.equipDescL2 ?? option) : option
        return (
          <option key={val} value={val}>
            {label}
          </option>
        )
      })}
    </select>
  )
}

export default SelectField
