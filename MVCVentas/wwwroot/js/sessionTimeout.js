function checkSession(){
    fetch("/Access/CheckSession")
        .then(response => {
            if(!response.ok){
                throw new Error("Error con la respuesta.");
            }
            return response.json();
        })
        .then(data => {
            if(data){
                console.log("La sesión está activa");
            }else{
                console.log("La sesión ha expirado");
                location.reload();
            }
            return data; // Retorna el valor booleano
        })
        .catch(error => {
            console.error("Error al verificar la sesión: ", error);
            return false; // Retorna false en caso de error
        })
        .finally(() => { // Llama a la facunción cada un minuto.
            setTimeout(checkSession, 60000); // 1 minuto
        });
}

// function checkSession(){
//     const now = Date.now();
//     const lastActivityTimestamp = localStorage.getItem('lastActivity');
//     const inactivityTime = now - lastActivityTimestamp;

//     if(inactivityTime > (1000 * 60 * 1)){
//         console.log("La sesión ha expirado");
//         location.reload();
//     }else{
//         console.log("La sesión está activa");
//         localStorage.setItem('lastActivity', now);
//     }
//     setTimeout(checkSession, 60000);
// }

// localStorage.setItem('lastActivity', Date.now());

checkSession();