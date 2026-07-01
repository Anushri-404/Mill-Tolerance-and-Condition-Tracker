import './inputs.css'

function TextInput({ value, onChange, disabled = false, type = 'text', name }) {
  return (
    <input
      className={
        'field-input' + (disabled ? ' field-input--disabled' : '')
      }
      type={type}
      name={name}
      value={value}
      disabled={disabled}
      onChange={onChange}
      min={type === 'number' ? 0 : undefined}
    />
  )
}

export default TextInput
