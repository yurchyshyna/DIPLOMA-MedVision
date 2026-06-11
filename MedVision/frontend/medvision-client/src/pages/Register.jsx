import { useState } from "react";
import axios from "axios";

export default function Register() {

    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");

    const register = async () => {

        try {

            await axios.post(
                "https://localhost:7281/api/Auth/register",
                {
                    email,
                    password
                }
            );

            alert("Реєстрація успішна!");

            window.location.href = "/";

        } catch (error) {

            console.error(error);

            alert("Користувач вже існує.");

        }

    };

    return (
        <div className="container mt-5">

            <h2 className="mb-4">
                Реєстрація
            </h2>

            <input
                type="email"
                className="form-control mb-3"
                placeholder="Email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
            />

            <input
                type="password"
                className="form-control mb-3"
                placeholder="Пароль"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
            />

            <button
                className="btn btn-success"
                onClick={register}
            >
                Зареєструватися
            </button>

        </div>
    );
}