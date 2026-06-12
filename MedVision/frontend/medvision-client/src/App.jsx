import { useEffect, useState } from "react";
import axios from "axios";
import Login from "./pages/Login";
import { pathologyInfo } from "./data/pathologyInfo";


function App() {
  const [selectedFile, setSelectedFile] = useState(null);
  const [preview, setPreview] = useState(null);

  const [result, setResult] = useState(null);
  const [loading, setLoading] = useState(false);
  const [history, setHistory] = useState([]);
  const [selectedAnalysis, setSelectedAnalysis] = useState(null);
  const [showModal, setShowModal] = useState(false);
  const [patientFullName, setPatientFullName] =
    useState("");

  const [selectedPathology, setSelectedPathology] =
    useState("");

  const [searchTerm, setSearchTerm] =
    useState("");

  const [patientBirthDate, setPatientBirthDate] =
    useState("");

  const [patientGender, setPatientGender] =
    useState("");

  const user =
    JSON.parse(
      localStorage.getItem("user")
    );

  if (!user) {
    return <Login />;
  }

  const API_URL =
    "https://localhost:7281/api/XrayAnalysis/upload";

  const HISTORY_URL =
    "https://localhost:7281/api/XrayAnalysis/history";

  const CLEAR_HISTORY_URL =
    "https://localhost:7281/api/XrayAnalysis/clear";



  // =========================
  // LOAD HISTORY
  // =========================

  const loadHistory = async () => {
    try {

      const user = JSON.parse(
        localStorage.getItem("user")
      );

      const response = await axios.get(
        `${HISTORY_URL}/${user.id}`
      );

      setHistory(response.data);

    } catch (error) {
      console.error(error);
    }
  };

  const clearHistory = async () => {

    const confirmClear = window.confirm(
      "Очистити всю історію аналізів?"
    );

    if (!confirmClear) return;

    try {

      await axios.delete(CLEAR_HISTORY_URL);

      setHistory([]);

    } catch (error) {

      console.error(error);

      alert("Помилка очищення історії.");

    }

  };

  const deleteAnalysis = async (id) => {

    const confirmed = window.confirm(
      "Видалити цей аналіз?"
    );

    if (!confirmed) return;

    try {

      const user = JSON.parse(
        localStorage.getItem("user")
      );

      await axios.delete(
        `https://localhost:7281/api/XrayAnalysis/${id}?userId=${user.id}`
      );

      await loadHistory();

    } catch (error) {

      console.error(error);

      alert(
        "Не вдалося видалити аналіз"
      );

    }

  };

  useEffect(() => {
    loadHistory();
  }, []);

  // =========================
  // FILE CHANGE
  // =========================

  const handleFileChange = async (event) => {

    const file = event.target.files[0];

    if (!file) return;

    const extension =
      file.name.split(".").pop().toLowerCase();

    const dicomFile =
      extension === "dcm" ||
      extension === "dicom";

    setSelectedFile(file);

    setResult(null);

    if (!dicomFile) {

      setPreview(
        URL.createObjectURL(file)
      );

      return;
    }

    try {

      const formData = new FormData();

      formData.append(
        "file",
        selectedFile
      );

      formData.append(
        "userId",
        user.id
      );

      formData.append(
        "patientFullName",
        patientFullName
      );

      formData.append(
        "patientBirthDate",
        patientBirthDate
      );

      formData.append(
        "patientGender",
        patientGender
      );

      const response = await axios.post(
        "http://127.0.0.1:8000/preview",
        formData,
        {
          responseType: "blob",
        }
      );

      const imageUrl =
        URL.createObjectURL(
          response.data
        );

      setPreview(imageUrl);

    } catch (error) {

      console.error(error);

      alert(
        "Помилка створення попереднього перегляду DICOM."
      );

    }

  };

  // =========================
  // ANALYZE
  // =========================

  const handleUpload = async () => {
    if (!selectedFile) {
      alert("Будь ласка, оберіть знімок.");
      return;
    }

    const formData = new FormData();

    formData.append("file", selectedFile);

    formData.append(
      "userId",
      user.id
    );


    formData.append(
      "patientFullName",
      patientFullName
    );

    formData.append(
      "patientBirthDate",
      patientBirthDate
    );

    formData.append(
      "patientGender",
      patientGender
    );

    try {
      setLoading(true);

      const response = await axios.post(
        API_URL,
        formData,
        {
          headers: {
            "Content-Type":
              "multipart/form-data",
          },
        }
      );

      setResult(response.data);

      await loadHistory();

    } catch (error) {
      console.error(error);

      alert("Помилка аналізу.");
    } finally {
      setLoading(false);
    }
  };

  const filteredHistory =
    history.filter((item) => {

      const matchesPatient =
        !searchTerm ||
        item.patientFullName
          ?.toLowerCase()
          .includes(
            searchTerm.toLowerCase()
          );

      const matchesPathology =
        !selectedPathology ||
        item.detectionsJson?.includes(
          selectedPathology
        );

      return (
        matchesPatient &&
        matchesPathology
      );

    });

  return (
    <div className="min-vh-100">

      {/* NAVBAR */}

      <nav className="navbar navbar-dark bg-primary shadow-sm">

        <div className="container">

          <span className="navbar-brand fw-bold fs-3">
            MedVision
          </span>

          <span className="text-white-50">
            AI Lung X-ray Analysis
          </span>

          <button
            className="btn btn-outline-light"
            onClick={() => {
              localStorage.removeItem("user");
              window.location.reload();
            }}
          >
            Вийти
          </button>

        </div>

      </nav>

      {/* ANALYSIS MODAL */}


      {showModal && selectedAnalysis && (

        <div
          className="modal fade show d-block"
          tabIndex="-1"
          style={{
            backgroundColor: "rgba(15, 35, 55, 0.35)",
            backdropFilter: "blur(8px)",
          }}
        >

          <div
            className="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable"
            style={{
              maxWidth: "1200px",
            }}
          >

            <div
              className="modal-content border-0 rounded-5 overflow-hidden"
              style={{
                background:
                  "linear-gradient(135deg, #eef5fb 0%, #dce8f3 100%)",
                color: "#10243e",
                boxShadow:
                  "0 25px 80px rgba(15, 23, 42, 0.25)",
              }}
            >

              {/* HEADER */}

              <div
                className="modal-header px-4 py-4"
                style={{
                  borderBottom: "1px solid #aac2d8",
                  background:
                    "linear-gradient(90deg, #0d2b45 0%, #134d74 100%)",
                }}
              >

                <h3
                  className="modal-title fw-bold"
                  style={{
                    color: "#e8f4ff",
                  }}
                >
                  Деталі аналізу
                </h3>

                <button
                  type="button"
                  className="btn-close"
                  onClick={() => setShowModal(false)}
                  style={{
                    filter: "invert(1)",
                  }}
                />

              </div>

              {/* BODY */}



              <div className="modal-body p-5">

                <div className="row g-4 align-items-start">

                  {/* ORIGINAL IMAGE */}

                  <div className="col-lg-4">

                    <div
                      className="p-3 rounded-5 h-100"
                      style={{
                        background: "#edf5fb",
                        boxShadow:
                          "0 10px 25px rgba(15,23,42,0.08)",
                      }}
                    >

                      <img
                        src={
                          selectedAnalysis.previewPath
                            ? `http://127.0.0.1:8000${selectedAnalysis.previewPath}`
                            : `https://localhost:7281${selectedAnalysis.imagePath}`
                        }
                        alt="X-ray"
                        className="img-fluid rounded-4"
                        style={{
                          width: "100%",
                          maxHeight: "420px",
                          objectFit: "contain",
                        }}
                      />

                    </div>

                  </div>

                  {/* HEATMAP */}

                  <div className="col-lg-4">

                    <div
                      className="p-3 rounded-5 h-100"
                      style={{
                        background: "#edf5fb",
                        boxShadow:
                          "0 10px 25px rgba(15,23,42,0.08)",
                      }}
                    >

                      {selectedAnalysis?.heatmapPath && (

                        <img
                          src={`http://127.0.0.1:8000${selectedAnalysis.heatmapPath}`}
                          alt="Heatmap"
                          className="img-fluid rounded-4"
                          style={{
                            width: "100%",
                            maxHeight: "420px",
                            objectFit: "contain",
                          }}
                        />

                      )}

                    </div>

                  </div>



                  {/* INFO PANEL */}

                  <div className="col-lg-4">



                    <div
                      className="rounded-5 p-4"
                      style={{
                        background: "#f8f8fb",
                        boxShadow:
                          "0 10px 30px rgba(15,23,42,0.08)",
                        border: "1px solid #d8e5f0",
                        minHeight: "420px",
                      }}
                    >

                      {/* TOP INFO */}

                      <div className="row text-center mb-4">

                        <div className="col-4">

                          <h6
                            className="fw-bold"
                            style={{
                              color: "#0b4f7a",
                            }}
                          >
                            Дата
                          </h6>

                          <small>
                            {new Date(
                              selectedAnalysis.createdAt
                            ).toLocaleString()}
                          </small>

                        </div>

                        <div className="col-4">

                          <h6
                            className="fw-bold"
                            style={{
                              color: "#0b4f7a",
                            }}
                          >
                            Клас
                          </h6>

                          <span
                            className={
                              selectedAnalysis.resultClass === "Abnormal"
                                ? "badge bg-danger"
                                : "badge bg-success"
                            }
                          >
                            {selectedAnalysis.resultClass}
                          </span>

                        </div>

                        <div className="col-4">

                          <h6
                            className="fw-bold"
                            style={{
                              color: "#0b4f7a",
                            }}
                          >
                            Ймовірність
                          </h6>

                          <strong>
                            {selectedAnalysis.probability}%
                          </strong>

                        </div>

                      </div>

                      <h5 className="fw-bold mt-4">
                        Дані пацієнта
                      </h5>

                      <p>
                        <strong>ПІБ:</strong>
                        {selectedAnalysis.patientFullName || "Не вказано"}
                      </p>

                      <p>
                        <strong>Дата народження:</strong>
                        {selectedAnalysis.patientBirthDate
                          ? new Date(
                            selectedAnalysis.patientBirthDate
                          ).toLocaleDateString()
                          : "Не вказано"}
                      </p>

                      <p>
                        <strong>Стать:</strong>
                        {selectedAnalysis.patientGender || "Не вказано"}
                      </p>

                      {/* DETECTIONS */}

                      <h2
                        className="mb-4"
                        style={{
                          color: "#1e2a78",
                        }}
                      >
                        Виявлені патології
                      </h2>

                      {selectedAnalysis?.detectionsJson &&
                        (() => {

                          try {
                            console.log(selectedAnalysis.detectionsJson);
                            const detections = JSON.parse(
                              selectedAnalysis.detectionsJson || "[]"
                            );



                            if (!detections.length) {

                              return (
                                <div className="alert alert-success">
                                  Патологій не виявлено
                                </div>
                              );

                            }

                            return (

                              <div>

                                {detections.map((d, index) => (

                                  <div
                                    key={index}
                                    className="rounded-4 p-3 mb-3"
                                    style={{
                                      background: "#ffffff",
                                      border: "1px solid #e2e8f0",
                                      boxShadow: "0 4px 12px rgba(0,0,0,0.05)",
                                    }}
                                  >

                                    <h5
                                      className="fw-bold mb-2"
                                      style={{
                                        color: "#c62828",
                                        fontSize: "20px",
                                      }}
                                    >
                                      {d.ClassName}
                                    </h5>

                                    <p
                                      className="mb-0"
                                      style={{
                                        color: "#334155",
                                        lineHeight: "1.6",
                                        fontSize: "15px",
                                      }}
                                    >
                                      {d.Description}
                                    </p>

                                  </div>

                                ))}

                              </div>

                            );

                          } catch {

                            return null;

                          }

                        })()}



                    </div>



                  </div>

                  <hr className="my-4" />

                  <h2
                    className="mb-4"
                    style={{
                      color: "#1e2a78",
                    }}
                  >
                    Медична довідка
                  </h2>

                  {selectedAnalysis?.detectionsJson &&
                    (() => {

                      try {

                        const detections = JSON.parse(
                          selectedAnalysis.detectionsJson || "[]"
                        );

                        return detections.map((d, index) => {

                          const info =
                            pathologyInfo[d.ClassName];

                          if (!info) return null;

                          return (

                            <div
                              key={index}
                              className="card border-0 mb-3"
                            >

                              <div className="card-body">

                                <h5 className="text-danger fw-bold">
                                  {info.title}
                                </h5>

                                <p>
                                  <strong>Опис:</strong>
                                  <br />
                                  {info.description}
                                </p>

                                <p>
                                  <strong>
                                    Характерні ознаки:
                                  </strong>
                                  <ul className="mt-2">
                                    {info.signs.map((sign, index) => (
                                      <li key={index}>
                                        {sign}
                                      </li>
                                    ))}
                                  </ul>
                                </p>

                              </div>

                            </div>

                          );

                        });

                      } catch {

                        return null;

                      }

                    })()}
                </div>

              </div>
            </div>
          </div>
        </div>

      )}

      {/* MAIN */}

      <main className="container-fluid px-5 py-5">

        <div className="row g-4">

          {/* LEFT COLUMN */}

          <div className="col-lg-4">

            <div className="card shadow border-0 rounded-4">

              <div className="card-body p-4">

                <h3 className="mb-3 text-primary">
                  Аналіз рентгенівського знімка
                </h3>

                <p className="text-muted">
                  Завантажте X-ray знімок для AI аналізу.
                </p>

                <input
                  type="file"
                  className="form-control"
                  accept=".png,.jpg,.jpeg,.dcm"
                  onChange={handleFileChange}
                />


                {preview && (

                  <div className="text-center mt-4">



                    <img
                      src={preview}
                      alt="Preview"
                      className="img-fluid rounded-4 shadow-sm"
                      style={{
                        maxHeight: "350px",
                      }}
                    />

                  </div>

                )}

                <div className="mt-4">

                  <label className="form-label fw-semibold">
                    ПІБ пацієнта
                  </label>

                  <input
                    type="text"
                    className="form-control"
                    value={patientFullName}
                    onChange={(e) =>
                      setPatientFullName(e.target.value)
                    }
                    placeholder="Іваненко Петро Сергійович"
                  />

                </div>

                <div className="mt-3">

                  <label className="form-label fw-semibold">
                    Дата народження
                  </label>

                  <input
                    type="date"
                    className="form-control"
                    value={patientBirthDate}
                    onChange={(e) =>
                      setPatientBirthDate(e.target.value)
                    }
                  />

                </div>

                <div className="mt-3">

                  <label className="form-label fw-semibold">
                    Стать
                  </label>

                  <select
                    className="form-select"
                    value={patientGender}
                    onChange={(e) =>
                      setPatientGender(e.target.value)
                    }
                  >
                    <option value="">
                      Не вказано
                    </option>

                    <option value="Чоловіча">
                      Чоловіча
                    </option>

                    <option value="Жіноча">
                      Жіноча
                    </option>

                  </select>

                </div>

                <button
                  className="btn btn-primary w-100 mt-4 py-2 fw-semibold"
                  onClick={handleUpload}
                  disabled={loading}
                >

                  {loading
                    ? "Аналіз..."
                    : "Аналізувати"}

                </button>

              </div>

            </div>

            {/* RESULT */}

            {result && (

              <div className="card shadow border-0 rounded-4 mt-4">

                <div className="card-body p-4">

                  <div className="mb-4">

                    <div className="d-flex justify-content-between mb-2">

                      <span className="text-muted">
                        Ймовірність
                      </span>

                      <strong>
                        {result.probability}%
                      </strong>

                    </div>

                    <div
                      className="progress"
                      style={{ height: "10px" }}
                    >

                      <div
                        className={
                          result.resultClass === "Abnormal"
                            ? "progress-bar bg-danger"
                            : "progress-bar bg-success"
                        }
                        role="progressbar"
                        style={{
                          width: `${result.probability}%`
                        }}
                      />

                    </div>

                  </div>

                  <div
                    className={
                      result.resultClass === "Abnormal"
                        ? "alert alert-danger"
                        : "alert alert-success"
                    }
                  >

                    <strong>Висновок:</strong>

                    <br />

                    {result.conclusion}

                    {result.detections &&
                      result.detections.length > 0 && (

                        <div className="mt-4">

                          {result.detections.map((d, index) => (

                            <div
                              key={index}
                              className="card border-danger mt-3"
                            >

                              <div className="card-body">

                                <h5 className="text-danger fw-bold">
                                  {d.className}
                                </h5>

                                <p className="mb-0">
                                  {d.description}
                                </p>

                              </div>

                            </div>

                          ))}

                        </div>

                      )}

                  </div>

                  {result.heatmapPath && (

                    <div className="mt-4">

                      <h5 className="text-primary mb-3">
                        Зображення з локалізацією патологій
                      </h5>

                      <img
                        src={`http://127.0.0.1:8000${result.heatmapPath}`}
                        alt="YOLO detection result"
                        className="img-fluid rounded-4 shadow"
                        style={{
                          maxHeight: "320px",
                          objectFit: "contain",
                        }}
                      />

                    </div>

                  )}

                  {result.resultClass === "Normal" && (

                    <div className="mt-4 alert alert-info">

                      AI не виявив ознак патологій
                      на рентгенівському знімку.

                    </div>

                  )}

                </div>

              </div>

            )}



          </div>

          {/* RIGHT COLUMN */}

          <div className="col-lg-8">

            <div className="card shadow border-0 rounded-4">

              <div className="card-body p-4">

                <div className="d-flex justify-content-between align-items-center mb-4">

                  <h3 className="text-primary mb-0">
                    Історія аналізів
                  </h3>

                  <button
                    className="btn btn-outline-primary"
                    onClick={clearHistory}
                  >
                    Очистити історію
                  </button>

                </div>

                <div className="row mb-4">

                  <div className="col-md-6">

                    <input
                      type="text"
                      className="form-control"
                      placeholder="🔍 Пошук пацієнта"
                      value={searchTerm}
                      onChange={(e) =>
                        setSearchTerm(e.target.value)
                      }
                    />

                  </div>

                  <div className="col-md-6">

                    <select
                      className="form-select"
                      value={selectedPathology}
                      onChange={(e) =>
                        setSelectedPathology(e.target.value)
                      }
                    >

                      <option value="Cardiomegaly">Cardiomegaly</option>
                      <option value="Pleural effusion">Pleural effusion</option>
                      <option value="Nodule/Mass">Nodule/Mass</option>
                      <option value="Pneumothorax">Pneumothorax</option>
                      <option value="Lung Opacity">Lung Opacity</option>
                      <option value="Consolidation">Consolidation</option>
                      <option value="Infiltration">Infiltration</option>
                      <option value="Pulmonary fibrosis">Pulmonary fibrosis</option>
                      <option value="Pleural thickening">Pleural thickening</option>
                      <option value="Calcification">Calcification</option>
                      <option value="Atelectasis">Atelectasis</option>
                      <option value="ILD">ILD</option>
                      <option value="Other lesion">Other lesion</option>

                    </select>

                  </div>

                </div>

                {history.length === 0 ? (

                  <div className="alert alert-light border">
                    Історія аналізів порожня.
                  </div>

                ) : (

                  <div className="table-responsive">

                    <table className="table align-middle">

                      <thead>

                        <tr>
                          <th>Знімок</th>
                          <th>Дата</th>
                          <th>Клас</th>
                          <th>Ймовірність</th>
                          <th>Дії</th>
                        </tr>

                      </thead>

                      <tbody>

                        {filteredHistory.map((item) => (

                          <tr
                            key={item.id}
                            style={{
                              cursor: "pointer"
                            }}
                            onClick={() => {
                              setSelectedAnalysis(item);
                              setShowModal(true);
                            }}
                          >

                            <td>

                              <img
                                src={
                                  item.previewPath
                                    ? `http://127.0.0.1:8000${item.previewPath}`
                                    : `https://localhost:7281${item.imagePath}`
                                }
                                alt="X-ray"
                                style={{
                                  width: "70px",
                                  height: "70px",
                                  objectFit: "cover",
                                  borderRadius: "8px",
                                }}
                              />

                            </td>

                            <td>
                              {new Date(
                                item.createdAt
                              ).toLocaleString()}
                            </td>

                            <td>

                              <span
                                className={
                                  item.resultClass === "Abnormal"
                                    ? "badge bg-danger"
                                    : "badge bg-success"
                                }
                              >
                                {item.resultClass}
                              </span>

                            </td>

                            <td>
                              {item.probability}%
                            </td>

                            <td>

                              <button
                                className="btn btn-outline-danger btn-sm"
                                onClick={(e) => {

                                  e.stopPropagation();

                                  deleteAnalysis(item.id);

                                }}
                              >
                                🗑
                              </button>

                            </td>

                          </tr>

                        ))}

                      </tbody>



                    </table>

                  </div>



                )}

              </div>



            </div>

            {result?.detections?.length > 0 && (

              <div className="card shadow border-0 rounded-4 mt-4">

                <div className="card-body p-4">

                  <h4 className="text-primary mb-4">
                    Медична довідка
                  </h4>

                  {result.detections.map((d, index) => {

                    const info =
                      pathologyInfo[d.className];

                    if (!info) return null;

                    return (

                      <div
                        key={index}
                        className="card border-0 mb-4"
                      >

                        <div className="card-body">

                          <h5 className="text-danger fw-bold">
                            {info.title}
                          </h5>

                          <p>
                            <strong>Опис:</strong>
                            <br />
                            {info.description}
                          </p>

                          <p>
                            <strong>
                              Характерні ознаки:
                            </strong>
                          </p>

                          <ul>
                            {info.signs.map(
                              (item, i) => (
                                <li key={i}>
                                  {item}
                                </li>
                              )
                            )}
                          </ul>

                          <p>
                            <strong>
                              Методи лікування:
                            </strong>
                          </p>

                          <ul>
                            {info.treatment.map(
                              (item, i) => (
                                <li key={i}>
                                  {item}
                                </li>
                              )
                            )}
                          </ul>

                          <p>
                            <strong>
                              Додаткові обстеження:
                            </strong>
                          </p>

                          <ul>
                            {info.additionalTests.map(
                              (item, i) => (
                                <li key={i}>
                                  {item}
                                </li>
                              )
                            )}
                          </ul>

                        </div>

                      </div>

                    );

                  })}

                </div>

              </div>

            )}

          </div>

        </div>

      </main>

      {/* FOOTER */}

      <footer className="bg-white border-top py-3 mt-5">

        <div className="container text-center text-muted">
          MedVision © 2026
        </div>

      </footer>

    </div>
  );
}

export default App;