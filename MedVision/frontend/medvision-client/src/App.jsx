import { useState } from "react";
import axios from "axios";

function App() {
  const [selectedFile, setSelectedFile] = useState(null);
  const [preview, setPreview] = useState(null);
  const [result, setResult] = useState(null);
  const [loading, setLoading] = useState(false);

  const API_URL = "https://localhost:7281/api/XrayAnalysis/upload";

  const handleFileChange = (event) => {
    const file = event.target.files[0];

    if (file) {
      setSelectedFile(file);
      setPreview(URL.createObjectURL(file));
      setResult(null);
    }
  };

  const handleUpload = async () => {
    if (!selectedFile) {
      alert("Будь ласка, оберіть рентгенівський знімок.");
      return;
    }

    const formData = new FormData();
    formData.append("file", selectedFile);

    try {
      setLoading(true);

      const response = await axios.post(API_URL, formData, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      });

      setResult(response.data);
    } catch (error) {
      console.error(error);
      alert("Помилка при аналізі знімка.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-vh-100 bg-light">
      <nav className="navbar navbar-dark bg-primary shadow-sm">
        <div className="container">
          <span className="navbar-brand fw-bold fs-3">MedVision</span>
          <span className="text-white-50">
            Computer Vision for Lung X-Ray Analysis
          </span>
        </div>
      </nav>

      <main className="container py-5">
        <div className="row align-items-center mb-5">
          <div className="col-lg-7">
            <h1 className="display-5 fw-bold text-primary">
              Інтерактивний аналіз рентгенівських знімків легень
            </h1>

            <p className="lead text-muted mt-3">
              Завантажте рентгенівський знімок легень, а система виконає
              автоматизований аналіз та сформує попередній результат.
            </p>

            <div className="alert alert-info mt-4">
              Система створена для демонстрації можливостей комп’ютерного зору
              та не є заміною професійної медичної діагностики.
            </div>
          </div>

          <div className="col-lg-5">
            <div className="card shadow border-0 rounded-4">
              <div className="card-body p-4">
                <h4 className="card-title mb-3">Завантаження знімка</h4>

                <input
                  type="file"
                  className="form-control"
                  accept="image/*"
                  onChange={handleFileChange}
                />

                {preview && (
                  <div className="text-center mt-4">
                    <img
                      src={preview}
                      alt="X-ray preview"
                      className="img-fluid rounded-3 shadow-sm"
                      style={{ maxHeight: "300px" }}
                    />
                  </div>
                )}

                <button
                  className="btn btn-primary w-100 mt-4 py-2 fw-semibold"
                  onClick={handleUpload}
                  disabled={loading}
                >
                  {loading ? "Виконується аналіз..." : "Аналізувати знімок"}
                </button>
              </div>
            </div>
          </div>
        </div>

        {loading && (
          <div className="text-center my-4">
            <div className="spinner-border text-primary" role="status"></div>
            <p className="mt-3 text-muted">Обробка рентгенівського знімка...</p>
          </div>
        )}

        {result && (
          <div className="card shadow border-0 rounded-4 mb-5">
            <div className="card-header bg-white border-0 p-4">
              <h3 className="mb-0 text-primary">Результат аналізу</h3>
            </div>

            <div className="card-body p-4">
              <div className="row g-4">
                <div className="col-md-4">
                  <div className="p-3 bg-light rounded-3 h-100">
                    <h6 className="text-muted">Клас результату</h6>
                    <p className="fs-5 fw-bold mb-0">
                      {result.resultClass}
                    </p>
                  </div>
                </div>

                <div className="col-md-4">
                  <div className="p-3 bg-light rounded-3 h-100">
                    <h6 className="text-muted">Ймовірність</h6>
                    <p className="fs-5 fw-bold mb-0">
                      {result.probability}%
                    </p>
                  </div>
                </div>

                <div className="col-md-4">
                  <div className="p-3 bg-light rounded-3 h-100">
                    <h6 className="text-muted">Дата аналізу</h6>
                    <p className="fs-6 fw-semibold mb-0">
                      {new Date(result.createdAt).toLocaleString()}
                    </p>
                  </div>
                </div>
              </div>

              <div className="alert alert-secondary mt-4 mb-0">
                <strong>Висновок:</strong> {result.conclusion}
              </div>
            </div>
          </div>
        )}
      </main>

      <footer className="bg-white border-top py-3">
        <div className="container text-center text-muted">
          MedVision © 2026 | Дипломний проєкт
        </div>
      </footer>
    </div>
  );
}

export default App;