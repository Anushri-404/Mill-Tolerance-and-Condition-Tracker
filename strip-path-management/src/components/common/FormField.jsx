import './FormField.css'

function FormField({ label, required = false, children, plain = false, className = '' }) {
  return (
    <div className={'form-field' + (className ? ' ' + className : '')}>
      <span className="form-field__label">
        {label}
        {required && <span className="required">*</span>}
        <span className="form-field__colon">:</span>
      </span>
      <span className={plain ? 'form-field__plain' : 'form-field__control'}>
        {children}
      </span>
    </div>
  )
}

export default FormField
