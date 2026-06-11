import { useState } from "react";
import axios from "axios";

export default function Login() {

    const [isRegister, setIsRegister] =
        useState(false);

    const [email, setEmail] =
        useState("");

    const [password, setPassword] =
        useState("");

    const login = async () => {

        try {

            const response =
                await axios.post(
                    "https://localhost:7281/api/Auth/login",
                    {
                        email,
                        password,
                    }
                );

            localStorage.setItem(
                "user",
                JSON.stringify(response.data)
            );

            window.location.reload();

        } catch {

            alert("Невірний логін або пароль");

        }

    };

    const register = async () => {

        try {

            await axios.post(
                "https://localhost:7281/api/Auth/register",
                {
                    email,
                    password,
                }
            );

            alert(
                "Реєстрація успішна. Тепер увійдіть."
            );

            setIsRegister(false);

        } catch {

            alert("Користувач вже існує");

        }

    };

    return (

        <div
            className="d-flex justify-content-center align-items-center"
            style={{
                minHeight: "100vh",
                backgroundImage: "url('/images/bakk.png')",
                backgroundSize: "cover",
                backgroundPosition: "center",
                backgroundRepeat: "no-repeat",
            }}
        >

            <div
                className="card border-0"
                style={{
                    width: "480px",
                    borderRadius: "30px",
                    background: "#ffffff",
                    boxShadow:
                        "0 20px 50px rgba(13,43,69,0.15)",
                }}
            >

                <div className="card-body p-5">

                    {/* LOGO */}

                    <div className="text-center mb-4">

                        <div
                            style={{
                                fontSize: "64px",
                            }}
                        >
                            🩺
                        </div>

                        <h1
                            className="fw-bold mb-2"
                            style={{
                                color: "#0d2b45",
                            }}
                        >
                            MedVision
                        </h1>

                        <p
                            className="mb-0"
                            style={{
                                color: "#6b7f93",
                            }}
                        >
                            AI Chest X-Ray Analysis
                        </p>

                    </div>

                    <h3
                        className="text-center mb-4 fw-semibold"
                        style={{
                            color: "#24557a",
                        }}
                    >
                        {isRegister
                            ? "Створення акаунта"
                            : "Вхід до системи"}
                    </h3>

                    {/* EMAIL */}

                    <input
                        type="email"
                        className="form-control mb-3"
                        placeholder="📧 Email"
                        value={email}
                        onChange={(e) =>
                            setEmail(e.target.value)
                        }
                        style={{
                            borderRadius: "15px",
                            padding: "14px",
                            border:
                                "1px solid #d3e0ea",
                        }}
                    />

                    {/* PASSWORD */}

                    <input
                        type="password"
                        className="form-control mb-4"
                        placeholder="🔒 Пароль"
                        value={password}
                        onChange={(e) =>
                            setPassword(e.target.value)
                        }
                        style={{
                            borderRadius: "15px",
                            padding: "14px",
                            border:
                                "1px solid #d3e0ea",
                        }}
                    />

                    {/* BUTTON */}

                    <button
                        className="btn w-100 text-white"
                        style={{
                            background:
                                "linear-gradient(90deg,#0d2b45,#24557a)",
                            border: "none",
                            borderRadius: "15px",
                            padding: "14px",
                            fontWeight: "600",
                            fontSize: "17px",
                        }}
                        onClick={
                            isRegister
                                ? register
                                : login
                        }
                    >

                        {isRegister
                            ? "Створити акаунт"
                            : "Увійти"}

                    </button>

                    {/* SWITCH */}

                    <div className="text-center mt-4">

                        {isRegister ? (

                            <>
                                <span
                                    style={{
                                        color: "#6b7f93",
                                    }}
                                >
                                    Вже маєте акаунт?
                                </span>

                                <button
                                    className="btn btn-link"
                                    onClick={() =>
                                        setIsRegister(false)
                                    }
                                >
                                    Увійти
                                </button>
                            </>

                        ) : (

                            <>
                                <span
                                    style={{
                                        color: "#6b7f93",
                                    }}
                                >
                                    Немає акаунта?
                                </span>

                                <button
                                    className="btn btn-link"
                                    onClick={() =>
                                        setIsRegister(true)
                                    }
                                >
                                    Зареєструватися
                                </button>
                            </>

                        )}

                    </div>

                </div>

            </div>

        </div>

    );

}