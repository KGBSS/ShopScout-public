let isEditing = false;
let originalValues = {};

function toggleEdit() {
    isEditing = true;
    const inputs = document.querySelectorAll('#profileForm input');
    inputs.forEach(input => {
        originalValues[input.id] = input.value;
        input.disabled = false;
    });
    document.getElementById('editBtn').classList.add('hidden');
    document.getElementById('actionButtons').classList.remove('hidden');
}

function cancelEdit() {
    isEditing = false;
    const inputs = document.querySelectorAll('#profileForm input');
    inputs.forEach(input => {
        input.value = originalValues[input.id];
        input.disabled = true;
    });
    document.getElementById('editBtn').classList.remove('hidden');
    document.getElementById('actionButtons').classList.add('hidden');
}
function toggleCheckbox(event) {
    event.preventDefault();
    const checkbox = document.getElementById('rememberMeCheckbox');
    const visualBtn = document.getElementById('customCheckbox');

    checkbox.checked = !checkbox.checked;

    checkbox.dispatchEvent(new Event('change', { bubbles: true }));

    if (checkbox.checked) {
        visualBtn.classList.add('checked');
    } else {
        visualBtn.classList.remove('checked');
    }
}

function downloadRecoveryCodes(recoveryCodes) {
    const text = 'Kétlépcsős hitelesítés helyreállítási kódjai\n\n' +
        recoveryCodes +
        '\n\nŐrizd meg biztonságosan. Minden kód csak egyszer használható fel.';

    const blob = new Blob([text], { type: 'text/plain' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'ShopScout-2fa-recovery-codes.txt';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
}

//document.addEventListener('DOMContentLoaded', function () {
    
//});