import './inputs.css'

function TextInput({ value, onChange, disabled = false, variant = 'default', type = 'text', name }) {
  const variantClass =
    variant === 'muted' ? ' field-input--muted' : variant === 'plain' ? ' field-input--plain' : ''

  return (
    <input
      className={'field-input' + variantClass + (disabled ? ' field-input--disabled' : '')}
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
