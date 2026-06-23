/* =============================================================
   MOCIDADE 015 — Front-end helpers
   - Toast region
   - Máscaras (telefone, CPF)
   - Confirmação de exclusão via modal Bootstrap
   - Mapa de assentos via delegação (submissão única)
   - Validação de força de senha
   ============================================================= */
(function () {
  "use strict";

  // -------------------- TOASTS --------------------
  function ensureToastRegion() {
    let region = document.getElementById("toast-region");
    if (!region) {
      region = document.createElement("div");
      region.id = "toast-region";
      region.className = "toast-region";
      region.setAttribute("aria-live", "polite");
      region.setAttribute("aria-atomic", "true");
      document.body.appendChild(region);
    }
    return region;
  }

  function showToast(message, type) {
    type = type || "info";
    const region = ensureToastRegion();
    const el = document.createElement("div");
    el.className = "app-toast app-toast--" + type;
    el.role = "status";

    const icon = {
      success: "check-circle-fill",
      danger: "exclamation-triangle-fill",
      warning: "exclamation-circle-fill",
      info: "info-circle-fill"
    }[type] || "info-circle-fill";

    el.innerHTML =
      '<i class="bi bi-' + icon + '" aria-hidden="true"></i>' +
      '<div class="flex-grow-1">' + message + "</div>" +
      '<button class="btn btn-link btn-sm p-0 ms-2 text-muted" aria-label="Fechar">&times;</button>';

    el.querySelector("button").addEventListener("click", function () {
      dismiss(el);
    });

    region.appendChild(el);
    setTimeout(function () { dismiss(el); }, 4500);
    return el;
  }

  function dismiss(el) {
    if (!el || el.classList.contains("is-leaving")) return;
    el.classList.add("is-leaving");
    setTimeout(function () { if (el.parentNode) el.parentNode.removeChild(el); }, 250);
  }

  window.toast = { show: showToast };

  // -------------------- MÁSCARAS --------------------
  function onlyDigits(value) { return (value || "").replace(/\D+/g, ""); }

  function maskPhone(value) {
    var d = onlyDigits(value).slice(0, 11);
    if (d.length <= 2) return d;
    if (d.length <= 6) return "(" + d.slice(0, 2) + ") " + d.slice(2);
    if (d.length <= 10) return "(" + d.slice(0, 2) + ") " + d.slice(2, 6) + "-" + d.slice(6);
    return "(" + d.slice(0, 2) + ") " + d.slice(2, 7) + "-" + d.slice(7);
  }

  function maskCpf(value) {
    var d = onlyDigits(value).slice(0, 11);
    if (d.length <= 3) return d;
    if (d.length <= 6) return d.slice(0, 3) + "." + d.slice(3);
    if (d.length <= 9) return d.slice(0, 3) + "." + d.slice(3, 6) + "." + d.slice(6);
    return d.slice(0, 3) + "." + d.slice(3, 6) + "." + d.slice(6, 9) + "-" + d.slice(9);
  }

  function attachMask(selector, fn) {
    document.querySelectorAll(selector).forEach(function (input) {
      input.addEventListener("input", function (e) {
        var pos = input.selectionStart;
        var before = input.value;
        input.value = fn(before);
        if (input.value.length > before.length) pos++;
        try { input.setSelectionRange(pos, pos); } catch (err) { /* noop */ }
      });
    });
  }

  attachMask('input[data-mask="phone"]', maskPhone);
  attachMask('input[data-mask="cpf"]', maskCpf);

  // -------------------- FORÇA DA SENHA --------------------
  function evaluatePassword(p) {
    if (!p) return { score: 0, label: "Vazia" };
    var score = 0;
    if (p.length >= 8) score++;
    if (p.length >= 12) score++;
    if (/[A-Z]/.test(p)) score++;
    if (/[0-9]/.test(p)) score++;
    if (/[^A-Za-z0-9]/.test(p)) score++;
    var labels = ["Muito fraca", "Fraca", "Razoável", "Boa", "Forte", "Muito forte"];
    return { score: score, label: labels[score] || "—" };
  }

  document.querySelectorAll('input[data-strength="true"]').forEach(function (input) {
    var hintId = input.id + "-strength";
    var hint = document.createElement("small");
    hint.id = hintId;
    hint.className = "form-text text-muted mt-1";
    input.insertAdjacentElement("afterend", hint);

    input.addEventListener("input", function () {
      var r = evaluatePassword(input.value);
      hint.textContent = "Força: " + r.label;
      hint.classList.remove("text-muted", "text-warning", "text-success");
      hint.classList.add(r.score <= 2 ? "text-warning" : "text-success");
    });
  });

  // -------------------- CONFIRMAÇÃO DE EXCLUSÃO --------------------
  document.querySelectorAll("form[data-confirm]").forEach(function (form) {
    form.addEventListener("submit", function (e) {
      var msg = form.getAttribute("data-confirm") || "Tem certeza?";
      if (!window.confirm(msg)) e.preventDefault();
    });
  });

  // -------------------- MAPA DE ASSENTOS (delegação) --------------------
  // Substitui N <form> por 1 form dinâmico acionado por clique no botão.
  (function () {
    var container = document.querySelector("[data-seat-map]");
    if (!container) return;

    var form = document.createElement("form");
    form.method = "post";
    form.action = window.location.pathname;
    form.style.display = "none";

    ["assentoId", "onibusId", "acompanhanteId"].forEach(function (name) {
      var i = document.createElement("input");
      i.type = "hidden";
      i.name = name;
      form.appendChild(i);
    });

    // Inclui antiforgery token se disponível
    var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    if (tokenInput) form.appendChild(tokenInput.cloneNode());

    document.body.appendChild(form);

    container.addEventListener("click", function (e) {
      var btn = e.target.closest("button.seat:not(:disabled)");
      if (!btn) return;
      e.preventDefault();

      form.querySelector('input[name="assentoId"]').value = btn.dataset.assentoId;
      form.querySelector('input[name="onibusId"]').value = btn.dataset.onibusId;
      form.querySelector('input[name="acompanhanteId"]').value = btn.dataset.acompanhanteId || "";

      btn.classList.add("seat--selected");
      btn.disabled = true;
      showToast("Reservando assento " + btn.dataset.label + "...", "info");

      // Dá tempo do feedback visual antes do submit
      setTimeout(function () { form.submit(); }, 220);
    });
  })();

  // -------------------- HIDRATAÇÃO DE TEMPDATA -> TOAST --------------------
  // Cada página pode declarar <div data-tempdata-toast="kind|message"></div> no TempData
  // e o JS converte em toast. Aqui só lemos um atributo data-toast-* no body.
  (function () {
    var data = document.body.dataset.toast;
    if (!data) return;
    try {
      var i = data.indexOf("|");
      var kind = data.slice(0, i);
      var msg = data.slice(i + 1);
      showToast(msg, kind);
    } catch (err) { /* noop */ }
  })();

  // -------------------- COPY-TO-CLIPBOARD --------------------
  document.querySelectorAll("[data-copy]").forEach(function (el) {
    el.addEventListener("click", function (e) {
      e.preventDefault();
      var text = el.dataset.copy;
      navigator.clipboard.writeText(text).then(function () {
        showToast("Copiado!", "success");
      });
    });
  });
})();
