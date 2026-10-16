export async function download(url, fileName) {
    const response = await fetch(url, {
        method: "GET",
        credentials: "same-origin",
        cache: "no-store"
    });

if (!response.ok)
{
    return response.status;
}

if (response.redirected)
{
    return 401;
}

const blob = await response.blob();
const objectUrl = URL.createObjectURL(blob);
const link = document.createElement("a");

link.href = objectUrl;
link.download = fileName;

document.body.appendChild(link);
link.click();
link.remove();

setTimeout(() => URL.revokeObjectURL(objectUrl), 60000);

return 200;
}