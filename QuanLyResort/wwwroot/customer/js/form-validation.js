/* Lightweight form validation and button behavior (no external deps) */
(function () {
  'use strict';

  function createInlineError(input, message) {
    let holder = input.closest('.form-group, .mb-3, .field') || input.parentElement;
    if (!holder) holder = input;
    let err = holder.querySelector('.fv-error');
    if (!err) {
      err = document.createElement('div');
      err.className = 'fv-error';
      err.style.cssText = 'color:#dc3545;font-size:12px;margin-top:4px;';
      holder.appendChild(err);
    }
    err.textContent = message || '';
    input.classList.add('is-invalid');
  }

  function clearInlineError(input) {
    input.classList.remove('is-invalid');
    const holder = input.closest('.form-group, .mb-3, .field') || input.parentElement;
    const err = holder && holder.querySelector('.fv-error');
    if (err) err.textContent = '';
  }

  function ensureSummary(form) {
    let summary = form.querySelector('.fv-summary');
    if (!summary) {
      summary = document.createElement('div');
      summary.className = 'fv-summary';
      summary.style.cssText = 'display:none;background:#fff3f3;border:1px solid #f5c2c7;color:#842029;border-radius:8px;padding:12px 14px;margin:0 0 16px';
      form.prepend(summary);
    }
    return summary;
  }

  function showSummary(form, messages) {
    const summary = ensureSummary(form);
    if (!messages || messages.length === 0) {
      summary.style.display = 'none';
      summary.innerHTML = '';
      return;
    }
    summary.style.display = 'block';
    summary.innerHTML = '<strong>Vui lòng kiểm tra lại:</strong><ul style="margin:8px 0 0 18px;">' +
      messages.map(m => `<li>${m}</li>`).join('') + '</ul>';
  }

  function validateInput(input) {
    clearInlineError(input);
    const label = (input.getAttribute('data-label') || input.name || 'Trường này').trim();
    const required = input.hasAttribute('required');
    const type = (input.type || '').toLowerCase();
    
    // Handle checkbox
    if (type === 'checkbox') {
      if (required && !input.checked) {
        return `${label} là bắt buộc`;
      }
      return '';
    }
    
    const value = (input.value || '').trim();

    if (required && !value) return `${label} là bắt buộc`;

    if (value) {
      if (type === 'email') {
        const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!re.test(value)) return `${label} không hợp lệ`;
      }
      const min = input.getAttribute('minlength');
      if (min && value.length < Number(min)) return `${label} tối thiểu ${min} ký tự`;
      const pattern = input.getAttribute('pattern');
      if (pattern) {
        try {
          const re = new RegExp(pattern);
          if (!re.test(value)) return `${label} không hợp lệ`;
        } catch (_) { /* ignore invalid pattern */ }
      }
    }
    return '';
  }

  function handleForm(form) {
    form.setAttribute('novalidate', 'novalidate');
    const fields = Array.from(form.querySelectorAll('input, textarea, select'));

    // Realtime feedback
    fields.forEach(el => {
      const events = el.type === 'checkbox' ? ['change', 'click'] : ['input', 'change', 'blur'];
      events.forEach(evt => {
        el.addEventListener(evt, () => {
          const msg = validateInput(el);
          if (msg) createInlineError(el, msg); else clearInlineError(el);
        });
      });
    });

    form.addEventListener('submit', (e) => {
      const messages = [];
      fields.forEach(el => {
        const msg = validateInput(el);
        if (msg) {
          messages.push(msg);
          createInlineError(el, msg);
        }
      });
      if (messages.length > 0) {
        e.preventDefault();
        showSummary(form, messages);
        const firstInvalid = form.querySelector('.is-invalid');
        if (firstInvalid) firstInvalid.focus({ preventScroll: false });
      } else {
        showSummary(form, []);
      }
    });
  }

  function wireButtons(root) {
    const scope = root || document;
    // Cancel/Close
    scope.querySelectorAll('[data-action="cancel"], [data-action="close"], .btn-cancel, .btn-close')
      .forEach(btn => btn.addEventListener('click', (e) => {
        const target = btn.getAttribute('data-target');
        if (target) {
          const el = document.querySelector(target);
          if (el) el.style.display = 'none';
        } else if (btn.closest('form')) {
          // Reset form and clear errors
          const f = btn.closest('form');
          f.reset();
          f.querySelectorAll('.is-invalid').forEach(i => i.classList.remove('is-invalid'));
          const sum = f.querySelector('.fv-summary');
          if (sum) { sum.style.display = 'none'; sum.innerHTML = ''; }
        } else {
          // Fallback: go back
          if (history.length > 1) history.back();
        }
      }));

    // Explicit submit wiring
    scope.querySelectorAll('[data-action="submit"]')
      .forEach(btn => btn.addEventListener('click', () => {
        const f = btn.closest('form');
        if (f) f.requestSubmit ? f.requestSubmit() : f.submit();
      }));
  }

  document.addEventListener('DOMContentLoaded', function () {
    // Auto bind for forms marked with data-validate="on"
    document.querySelectorAll('form[data-validate="on"]').forEach(handleForm);
    wireButtons(document);
  });
  
  // Expose validation functions globally for use in other scripts
  window.formValidation = {
    validateInput: validateInput,
    createInlineError: createInlineError,
    clearInlineError: clearInlineError,
    showSummary: showSummary
  };
})();


