import './inputs.css'

function DateInput({ value, onChange, name, variant = 'default' }) {
  const variantClass = variant === 'muted' ? ' field-input--muted' : ''
  return (
    <div className="date-input">
      <input
        className={'field-input date-input__field' + variantClass}
        type="date"
        name={name}
        value={value}
        onChange={onChange}
      />
      <span className="date-input__icon" aria-hidden="true">
        📅
      </span>
    </div>
  )
}

export default DateInput
