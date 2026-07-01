import './inputs.css'

function FileInput({ onChange, name }) {
  return <input className="field-file" type="file" name={name} onChange={onChange} />
}

export default FileInput
