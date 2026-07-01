import './inputs.css'

function TextArea({ value, onChange, name, rows = 3 }) {
  return (
    <textarea
      className="field-textarea"
      name={name}
      rows={rows}
      value={value}
      onChange={onChange}
    />
  )
}

export default TextArea
