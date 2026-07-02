import './inputs.css'

function TextArea({ value, onChange, name, rows = 3, variant = 'default' }) {
  const variantClass = variant === 'plain' ? ' field-input--plain' : ''
  return (
    <textarea
      className={'field-textarea' + variantClass}
      name={name}
      rows={rows}
      value={value}
      onChange={onChange}
    />
  )
}

export default TextArea
