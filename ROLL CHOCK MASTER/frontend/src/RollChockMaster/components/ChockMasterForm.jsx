import { useState, useEffect, useRef, forwardRef, useImperativeHandle, Fragment } from "react";
import alertify from "alertifyjs";
import "alertifyjs/build/css/alertify.css";
import {
  DIAMETER_ROWS,
  WIDTH_ROWS,
  widthField,
  WITHOUT_LINER_PREFIX,
  WITH_LINER_PREFIX,
} from "../chockFieldsConfig";
import {
  fetchChockLookups,
  fetchExistingChock,
  fetchChockTypeConfig,
  saveChock,
} from "../services/chockLookupService";
import "./ChockMasterForm.css";

const EDITABLE_STATUS = "CNEW";

const ALL_NUMERIC_FIELDS = [
  ...DIAMETER_ROWS.flatMap((r) => [r.aField, r.bField]),
  ...WIDTH_ROWS.flatMap((r) => [
    widthField(WITHOUT_LINER_PREFIX, r.suffix, "IN"),
    widthField(WITHOUT_LINER_PREFIX, r.suffix, "OUT"),
    widthField(WITH_LINER_PREFIX, r.suffix, "IN"),
    widthField(WITH_LINER_PREFIX, r.suffix, "OUT"),
  ]),
  "CHM_CHK_LIN_SZ_1",
];

const emptyFormState = () => {
  const base = {
    CHM_CHK_TYP: "",
    CHM_ID_CHOCK: "",
    CHM_CD_CHK_PROG: "",
    CHM_DT_CHK_IMP: "",
    CHM_CHK_MAKER: "",
    CHM_REMARKS: "",
  };
  ALL_NUMERIC_FIELDS.forEach((f) => (base[f] = ""));
  return base;
};

const emptyTolerance = () => ({
  CHS_CK_IDI_TL_U: "",
  CHS_CK_IDI_TL_L: "",
  CHS_CK_END_TL_U: "",
  CHS_CK_END_TL_L: "",
  CHS_CK_W_LIN_TL_U: "",
  CHS_CK_W_LIN_TL_L: "",
  CHS_CK_W_LIN_TL_U1: "",
  CHS_CK_W_LIN_TL_L1: "",
  CHS_CK_LIN_TL_U: "",
  CHS_CK_LIN_TL_L: "",
  CHS_CK_LIN_TL_U1: "",
  CHS_CK_LIN_TL_L1: "",
});

const ChockMasterForm = forwardRef((props, ref) => {
  const [form, setForm] = useState(emptyFormState);
  const [tolerance, setTolerance] = useState(emptyTolerance);
  const [statusDesc, setStatusDesc] = useState("");
  const [isExistingRecord, setIsExistingRecord] = useState(false);
  const [lookups, setLookups] = useState({ chockType: [], chockMaker: [] });
  const [saving, setSaving] = useState(false);
  // Field names (matching T_CHOCK_MAST columns) that don't apply to the
  // currently selected Chock Type — driven by which T_CHOCK_STND tolerance
  // columns are NULL for that type. These render disabled/greyed and are
  // excluded from validation and cleared before save.
  const [disabledFields, setDisabledFields] = useState([]);

  // Guards against an in-flight Chock ID lookup landing after the type/id
  // have already changed again.
  const lookupToken = useRef(0);

  useEffect(() => {
    fetchChockLookups()
      .then(setLookups)
      .catch((err) => {
        console.error("Error fetching chock lookups:", err);
        alertify.error("Could not load Chock Type / Chock Maker lists.");
      });
  }, []);

  const set = (field) => (e) =>
    setForm((prev) => ({ ...prev, [field]: e.target.value }));

  const mapTolerance = (t) => {
    const out = {};
    Object.keys(emptyTolerance()).forEach((k) => {
      out[k] = t[k] ?? t[toCamel(k)] ?? "";
    });
    return out;
  };

  // Clears any currently-entered values for fields that just became
  // not-applicable for the selected Chock Type, so stale numbers from a
  // previous type don't silently get saved against a field that no longer
  // has a tolerance standard behind it.
  const clearFieldsNowDisabled = (fields) => {
    if (!fields.length) return;
    setForm((prev) => {
      const next = { ...prev };
      fields.forEach((f) => {
        next[f] = "";
      });
      return next;
    });
  };

  // Fires when the Chock Type dropdown changes. This is what makes the
  // "select a type -> some boxes go grey, status auto-fills" behavior work:
  // it pulls the tolerance standard for the type and applies both the
  // disabled-field set and (for brand-new chocks) the default status.
  const handleChockTypeChange = async (e) => {
    const chockType = e.target.value;
    setForm((prev) => ({ ...prev, CHM_CHK_TYP: chockType }));

    if (!chockType) {
      setTolerance(emptyTolerance());
      setDisabledFields([]);
      return;
    }

    const myToken = ++lookupToken.current;
    try {
      const config = await fetchChockTypeConfig(chockType, form.CHM_ID_CHOCK || undefined);
      if (myToken !== lookupToken.current) return; // a newer change superseded this one

      setTolerance(config.tolerance ? mapTolerance(config.tolerance) : emptyTolerance());
      setDisabledFields(config.disabledFields);
      clearFieldsNowDisabled(config.disabledFields);

      // Only auto-set the status for a chock we're not already editing —
      // an existing record keeps whatever status it actually has.
      if (!isExistingRecord) {
        setForm((prev) => ({ ...prev, CHM_CD_CHK_PROG: config.defaultStatusCode }));
        setStatusDesc(config.defaultStatusDesc || "");
      }
    } catch (err) {
      console.error("Error fetching chock type configuration:", err);
      alertify.error("Could not load field rules for this Chock Type.");
    }
  };

  // Shared lookup used by both the explicit Query action and the silent
  // auto-lookup on Chock ID blur. `announce` controls whether not-found /
  // error states get an alertify message — Query is a deliberate user
  // action so it always speaks up; the blur auto-lookup stays quiet.
  const runLookup = async (chockId, chockType, { announce }) => {
    const myToken = ++lookupToken.current;
    try {
      const [existing, config] = await Promise.all([
        fetchExistingChock(chockId, chockType),
        fetchChockTypeConfig(chockType, chockId),
      ]);
      if (myToken !== lookupToken.current) return;

      setTolerance(config.tolerance ? mapTolerance(config.tolerance) : emptyTolerance());
      setDisabledFields(config.disabledFields);

      if (existing) {
        setForm((prev) => ({ ...prev, ...existing.record }));
        setStatusDesc(existing.statusDesc ?? "");
        if (existing.tolerance) setTolerance(mapTolerance(existing.tolerance));
        setIsExistingRecord(true);
      } else {
        setIsExistingRecord(false);
        clearFieldsNowDisabled(config.disabledFields);
        if (announce) {
          alertify.error("Record not found — enter details to create a new chock");
        }
      }
    } catch (err) {
      console.error("Error looking up chock:", err);
      if (announce) {
        alertify.error("Query failed — check the backend connection.");
      }
    }
  };

  // Explicit "Query" action (top action bar). Mirrors the legacy
  // EXECUTE-QUERY trigger: requires both Chock Type and Chock ID, and
  // reports outright if nothing is found.
  const handleQuery = async () => {
    if (!form.CHM_CHK_TYP || !form.CHM_ID_CHOCK) {
      alertify.error("Enter Chock Type and Chock ID");
      return;
    }
    await runLookup(form.CHM_ID_CHOCK, form.CHM_CHK_TYP, { announce: true });
  };

  // Fires when the user tabs/clicks out of the Chock ID field. Runs the
  // same lookup silently in the background so a matching record loads
  // automatically without requiring a manual Query click.
  const handleChockIdBlur = async () => {
    const chockId = form.CHM_ID_CHOCK;
    const chockType = form.CHM_CHK_TYP;
    if (!chockId || !chockType) return;
    await runLookup(chockId, chockType, { announce: false });
  };

  const validate = () => {
    if (!form.CHM_CHK_TYP) {
      alertify.error("Select a Chock Type");
      return false;
    }
    if (!form.CHM_ID_CHOCK) {
      alertify.error("Enter Chock ID");
      return false;
    }
    if (!form.CHM_CHK_MAKER) {
      alertify.error("Enter Chock Maker");
      return false;
    }
    if (isExistingRecord && form.CHM_CD_CHK_PROG && form.CHM_CD_CHK_PROG !== EDITABLE_STATUS) {
      alertify.error(`You can modify chock with status ${EDITABLE_STATUS} only`);
      return false;
    }
    return true;
  };

  const buildPayload = () => {
    const payload = { ...form };
    ALL_NUMERIC_FIELDS.forEach((f) => {
      // Fields not applicable to this Chock Type are always sent as null,
      // regardless of anything left over in local state.
      payload[f] = disabledFields.includes(f) || form[f] === "" ? null : Number(form[f]);
    });
    return payload;
  };

  const handleSave = async () => {
    if (!validate()) return;
    setSaving(true);
    try {
      const result = await saveChock(buildPayload());
      if (result.success) {
        alertify.success(result.wasUpdate ? "Record Updated" : "Record Inserted");
        setIsExistingRecord(true);
      }
    } catch (err) {
      alertify.error(err.message || "Record Failed");
    } finally {
      setSaving(false);
    }
  };

  const clearAll = () => {
    setForm(emptyFormState());
    setTolerance(emptyTolerance());
    setStatusDesc("");
    setIsExistingRecord(false);
    setDisabledFields([]);
  };

  useImperativeHandle(ref, () => ({
    query: handleQuery,
    save: handleSave,
    clear: clearAll,
  }));

  const isDisabled = (field) => disabledFields.includes(field);

  // One row of the Chock Width table — shared by "without Liner" and "with Liner".
  // upperField/lowerField (when present) pull real tolerance values in for the
  // two rows flagged hasTolerance (Top -> *_U/_L, Top Lower -> *_U1/_L1).
  const renderWidthRow = (row, prefix, upperField, lowerField) => {
    const inField = widthField(prefix, row.suffix, "IN");
    const outField = widthField(prefix, row.suffix, "OUT");
    const inDisabled = isDisabled(inField);
    const outDisabled = isDisabled(outField);
    return (
      <div className="crf-width-row" key={inField}>
        <span className="crf-width-label">{row.label}</span>
        <div className="crf-width-cell">
          <input
            className={`crf-input crf-input--num${inDisabled ? " crf-input--disabled" : ""}`}
            value={form[inField]}
            onChange={set(inField)}
            disabled={inDisabled}
          />
          <span className="crf-unit">(mm)</span>
        </div>
        <div className="crf-width-cell">
          <input
            className={`crf-input crf-input--num${outDisabled ? " crf-input--disabled" : ""}`}
            value={form[outField]}
            onChange={set(outField)}
            disabled={outDisabled}
          />
          <span className="crf-unit">(mm)</span>
        </div>
        <div className="crf-width-cell">
          {row.hasTolerance && (
            <>
              <input
                className="crf-input crf-input--num crf-input--disabled crf-input--limit"
                value={tolerance[lowerField] ?? ""}
                disabled
                readOnly
              />
              <span className="crf-unit">(mm)</span>
            </>
          )}
        </div>
        <div className="crf-width-cell">
          {row.hasTolerance && (
            <>
              <input
                className="crf-input crf-input--num crf-input--disabled crf-input--limit"
                value={tolerance[upperField] ?? ""}
                disabled
                readOnly
              />
              <span className="crf-unit">(mm)</span>
            </>
          )}
        </div>
      </div>
    );
  };

  return (
    <div className="crf-screen">
      {/* Chock Information */}
      <fieldset className="crf-panel crf-panel--inner">
        <legend>Chock Information</legend>
        <div className="crf-info-grid">
          {/* Row 1 */}
          <label className="crf-label crf-label--link">Chock Type</label>
          <select className="crf-input crf-input--num" value={form.CHM_CHK_TYP} onChange={handleChockTypeChange}>
            <option value="">--Select--</option>
            {(lookups?.chockType ?? []).map((t) => (
              <option key={t} value={t}>{t}</option>
            ))}
          </select>

          <label className="crf-label">Status Code</label>
          <div className="crf-status-group">
            <input className="crf-input crf-input--num" value={form.CHM_CD_CHK_PROG} readOnly />
            <input className="crf-input crf-input--wide" value={statusDesc} readOnly />
          </div>

          {/* Row 2 */}
          <label className="crf-label crf-label--link">Chock ID</label>
          <input
            className="crf-input crf-input--num"
            value={form.CHM_ID_CHOCK}
            onChange={set("CHM_ID_CHOCK")}
            onBlur={handleChockIdBlur}
            maxLength={6}
          />

          <label className="crf-label">Receipt Date</label>
          <input className="crf-input crf-input--num" type="date" value={form.CHM_DT_CHK_IMP ?? ""} onChange={set("CHM_DT_CHK_IMP")} />

          <label className="crf-label crf-chockmaker">Chock Maker</label>
          <select className="crf-input crf-input--wide crf-chockmaker" value={form.CHM_CHK_MAKER} onChange={set("CHM_CHK_MAKER")}>
            <option value="">--Select--</option>
            {(lookups?.chockMaker ?? []).map((m) => (
              <option key={m.codeValue} value={m.codeValue}>{m.codeDesc || m.codeValue}</option>
            ))}
          </select>
        </div>
      </fieldset>

      <div className="crf-row-panels">
        {/* Inner Chock Diameter */}
        <fieldset className="crf-panel crf-panel--inner crf-panel--half">
          <legend>Inner Chock Diameter</legend>
          <div className="crf-diameter-grid">
            <span />
            <span className="crf-diameter-head">A (LZ)</span>
            <span className="crf-diameter-head">B (NLZ)</span>
            <span className="crf-diameter-head crf-label--lower">Lower Limit</span>
            <span className="crf-diameter-head crf-label--upper">Upper Limit</span>

            {DIAMETER_ROWS.map((r, idx) => {
              const aDisabled = isDisabled(r.aField);
              const bDisabled = isDisabled(r.bField);
              return (
                <Fragment key={r.row}>
                  <span className="crf-row-num">{r.row}</span>
                  <span className="crf-unit-wrap">
                    <input
                      className={`crf-input crf-input--num${aDisabled ? " crf-input--disabled" : ""}`}
                      value={form[r.aField]}
                      onChange={set(r.aField)}
                      disabled={aDisabled}
                    />
                    <span className="crf-unit">(mm)</span>
                  </span>
                  <span className="crf-unit-wrap">
                    <input
                      className={`crf-input crf-input--num${bDisabled ? " crf-input--disabled" : ""}`}
                      value={form[r.bField]}
                      onChange={set(r.bField)}
                      disabled={bDisabled}
                    />
                    <span className="crf-unit">(mm)</span>
                  </span>
                  {idx === 0 ? (
                    <>
                      <span className="crf-unit-wrap">
                        <input
                          className="crf-input crf-input--num crf-input--disabled crf-input--limit"
                          value={tolerance.CHS_CK_IDI_TL_L ?? ""}
                          disabled
                          readOnly
                        />
                        <span className="crf-unit">(mm)</span>
                      </span>
                      <span className="crf-unit-wrap">
                        <input
                          className="crf-input crf-input--num crf-input--disabled crf-input--limit"
                          value={tolerance.CHS_CK_IDI_TL_U ?? ""}
                          disabled
                          readOnly
                        />
                        <span className="crf-unit">(mm)</span>
                      </span>
                    </>
                  ) : (
                    <>
                      <span />
                      <span />
                    </>
                  )}
                </Fragment>
              );
            })}
          </div>
        </fieldset>

        {/* End Cover */}
        <fieldset className="crf-panel crf-panel--inner crf-panel--half">
          <legend>End Cover</legend>
          <div className="crf-endcover-layout">
            <div className="crf-endcover-main">
              <label className="crf-label">End Cover Ht</label>
              <span className="crf-unit-wrap">
                <input className="crf-input crf-input--num crf-input--disabled" disabled />
                <span className="crf-unit">(mm)</span>
              </span>
            </div>
            <div className="crf-endcover-tolerance-box">
              <div className="crf-endcover-tolerance-title">Tolerance Limit</div>
              <div className="crf-endcover-tolerance-subheader">
                <span className="crf-label--lower">Lower</span>
                <span className="crf-label--upper">Upper</span>
              </div>
              <div className="crf-endcover-tolerance-inputs">
                <span className="crf-unit-wrap">
                  <input
                    className="crf-input crf-input--num crf-input--disabled crf-input--limit"
                    value={tolerance.CHS_CK_END_TL_L ?? ""}
                    disabled
                    readOnly
                  />
                  <span className="crf-unit">(mm)</span>
                </span>
                <span className="crf-unit-wrap">
                  <input
                    className="crf-input crf-input--num crf-input--disabled crf-input--limit"
                    value={tolerance.CHS_CK_END_TL_U ?? ""}
                    disabled
                    readOnly
                  />
                  <span className="crf-unit">(mm)</span>
                </span>
              </div>
            </div>
          </div>
        </fieldset>
      </div>

      <div className="crf-row-panels">
        {/* Chock Width without Liner */}
        <fieldset className="crf-panel crf-panel--inner crf-panel--half">
          <legend>Chock Width without Liner</legend>
          <div className="crf-width-header">
            <span />
            <span>Inboard</span>
            <span>Outboard</span>
            <span className="crf-label--lower">Lower</span>
            <span className="crf-label--upper">Upper</span>
          </div>
          {WIDTH_ROWS.map((r, idx) =>
            renderWidthRow(
              r,
              WITHOUT_LINER_PREFIX,
              idx === 0 ? "CHS_CK_W_LIN_TL_U" : "CHS_CK_W_LIN_TL_U1",
              idx === 0 ? "CHS_CK_W_LIN_TL_L" : "CHS_CK_W_LIN_TL_L1"
            )
          )}
        </fieldset>

        {/* Chock Width with Liner */}
        <fieldset className="crf-panel crf-panel--inner crf-panel--half">
          <legend>Chock Width with Liner</legend>
          <div className="crf-width-header">
            <span />
            <span>Inboard</span>
            <span>Outboard</span>
            <span className="crf-label--lower">Lower</span>
            <span className="crf-label--upper">Upper</span>
          </div>
          {WIDTH_ROWS.map((r, idx) =>
            renderWidthRow(
              r,
              WITH_LINER_PREFIX,
              idx === 0 ? "CHS_CK_LIN_TL_U" : "CHS_CK_LIN_TL_U1",
              idx === 0 ? "CHS_CK_LIN_TL_L" : "CHS_CK_LIN_TL_L1"
            )
          )}
        </fieldset>
      </div>

      <fieldset className="crf-panel crf-panel--outer">
        <legend>Remarks</legend>
        <textarea
          className="crf-input crf-textarea"
          value={form.CHM_REMARKS}
          onChange={set("CHM_REMARKS")}
          maxLength={300}
        />
      </fieldset>

      <div className="crf-save-row">
        <button
          type="button"
          className="crf-save-btn"
          onClick={handleSave}
          disabled={saving}
        >
          {saving ? "Saving…" : "Save"}
        </button>
      </div>
    </div>
  );
});

function toCamel(s) {
  return s
    .toLowerCase()
    .replace(/_([a-z0-9])/g, (_, c) => c.toUpperCase());
}

ChockMasterForm.displayName = "ChockMasterForm";

export default ChockMasterForm;
