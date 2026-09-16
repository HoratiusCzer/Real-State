"use client";

/** fetch() doesn't expose upload-progress events in the browsers this targets — XHR is still the
 * standard way to get them (spec §8.2: "upload progress"). Posts to the same-origin proxy routes
 * (app/api/portal/listings/[id]/media|documents), which attach the caller's access token
 * server-side — the browser never handles the token itself. */
export function uploadWithProgress(
  url: string,
  formData: FormData,
  onProgress: (percent: number) => void
): Promise<{ ok: boolean; status: number; body: unknown }> {
  return new Promise((resolve, reject) => {
    const xhr = new XMLHttpRequest();
    xhr.open("POST", url);
    xhr.upload.onprogress = (e) => {
      if (e.lengthComputable) onProgress(Math.round((e.loaded / e.total) * 100));
    };
    xhr.onload = () => {
      let body: unknown = null;
      try {
        body = JSON.parse(xhr.responseText);
      } catch {
        // no JSON body
      }
      resolve({ ok: xhr.status >= 200 && xhr.status < 300, status: xhr.status, body });
    };
    xhr.onerror = () => reject(new Error("Upload failed."));
    xhr.send(formData);
  });
}
