const result = document.getElementById("result");

function show(value) {
    result.textContent = typeof value === "string"
        ? value
        : JSON.stringify(value, null, 2);
}

async function sendJson(url, body) {
    const response = await fetch(url, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(body)
    });

    const text = await response.text();
    const data = text ? JSON.parse(text) : null;

    if (!response.ok) {
        throw {
            status: response.status,
            body: data
        };
    }

    return data;
}

async function getWithAuth(url) {
    const token = localStorage.getItem("accessToken");

    const response = await fetch(url, {
        method: "GET",
        headers: {
            "Authorization": `Bearer ${token}`
        }
    });

    const text = await response.text();
    const data = text ? JSON.parse(text) : null;

    if (!response.ok) {
        throw {
            status: response.status,
            body: data
        };
    }

    return data;
}

async function postWithAuth(url) {
    const token = localStorage.getItem("accessToken");

    const response = await fetch(url, {
        method: "POST",
        headers: {
            "Authorization": `Bearer ${token}`
        }
    });

    const text = await response.text();
    const data = text ? JSON.parse(text) : null;

    if (!response.ok) {
        throw {
            status: response.status,
            body: data
        };
    }

    return data;
}

document.getElementById("registerButton").addEventListener("click", async () => {
    try {
        const email = document.getElementById("registerEmail").value;
        const password = document.getElementById("registerPassword").value;

        const data = await sendJson("/api/Auth/register", {
            email,
            password,
            confirmPassword: password
        });

        show(data);
    } catch (error) {
        show(error);
    }
});

document.getElementById("loginButton").addEventListener("click", async () => {
    try {
        const email = document.getElementById("loginEmail").value;
        const password = document.getElementById("loginPassword").value;

        const data = await sendJson("/api/Auth/login", {
            email,
            password
        });

        if (data.accessToken) {
            localStorage.setItem("accessToken", data.accessToken);
        }

        show(data);
    } catch (error) {
        show(error);
    }
});

document.getElementById("meButton").addEventListener("click", async () => {
    try {
        const data = await getWithAuth("/api/Auth/me");
        show(data);
    } catch (error) {
        show(error);
    }
});

document.getElementById("logoutButton").addEventListener("click", async () => {
    try {
        const data = await postWithAuth("/api/Auth/logout");
        localStorage.removeItem("accessToken");
        show(data);
    } catch (error) {
        show(error);
    }
});
