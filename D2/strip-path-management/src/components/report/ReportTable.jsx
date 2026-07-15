import './ReportTable.css'

const COLUMNS = [
  { key: 'downloadAttachment', label: 'Download Attachment' },
  { key: 'observationId', label: 'OBSERVATION_ID' },
  { key: 'section', label: 'SECTION' },
  { key: 'equipLv1', label: 'EQUIP_LV1' },
  { key: 'equipLv2Id', label: 'EQUIP_LV2_ID' },
  { key: 'equipLv2Desc', label: 'EQUIP_LV2_DESC' },
  { key: 'observation', label: 'OBSERVATION' },
  { key: 'affectedPortion', label: 'AFFECTETED_PORTION' },
  { key: 'defectDetails', label: 'DEFECT_DETAILS' },
  { key: 'diameterActual', label: 'DIAMETER_ACTUAL' },
  { key: 'rollcoatActual', label: 'ROLLCOAT_ACTUAL' },
  { key: 'rollTouchpoint', label: 'ROLL_TOUCHPOINT' },
  { key: 'hardnessActual', label: 'HARNESS_ACTUAL' },
  { key: 'maintenancePhilosophy', label: 'MAINTENANCE_PHILOSOPHY' },
  { key: 'replacementFrequency', label: 'REPLACEMENT_FREQUENCY' },
  { key: 'diameterNew', label: 'DIAMETER_NEW' },
  { key: 'hardnessNew', label: 'HARDNESS_NEW' },
  { key: 'liningCondNew', label: 'LINING_COND_NEW' },
  { key: 'bearingCondNew', label: 'BEARING_COND_NEW' },
  { key: 'bakeliteGuideplateCond', label: 'BAKELITE_GUIDEPLATE_COND' },
  { key: 'status', label: 'STATUS' },
  { key: 'stripPathAuditDate', label: 'STRIPPATH_AUDIT_DATE' },
  { key: 'lastRollchangeDate', label: 'LAST_ROLLCHANGE_DATE' },
  { key: 'lastBearingGreasingDate', label: 'LAST_BEARING_GREASING_DATE' },
  { key: 'loggedOn', label: 'LOGGEDON' },
]

function ReportTable({ rows = [] }) {
  return (
    <div className="report-table__wrapper">
      <table className="report-table">
        <thead>
          <tr>
            {COLUMNS.map((col) => (
              <th key={col.key}>{col.label}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows.length === 0 ? (
            <tr>
              <td className="report-table__empty" colSpan={COLUMNS.length}>
                No data. Set filters and click View.
              </td>
            </tr>
          ) : (
            rows.map((row, idx) => (
              <tr key={row.observationId ?? idx}>
                {COLUMNS.map((col) => (
                  <td key={col.key}>
                    {col.key === 'downloadAttachment' ? (
                      row.attachmentUrl ? (
                        <a href={row.attachmentUrl} target="_blank" rel="noreferrer">
                          Download
                        </a>
                      ) : (
                        'NA'
                      )
                    ) : (
                      row[col.key] ?? ''
                    )}
                  </td>
                ))}
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  )
}

export default ReportTable