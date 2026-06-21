const provinceSelect = document.getElementById("province");
const wardSelect = document.getElementById("ward");

// 1. Load tất cả tỉnh
async function loadProvinces() {
    const res = await fetch("https://provinces.open-api.vn/api/v2/p/");
    const data = await res.json();

    provinceSelect.innerHTML = `<option value="">Chọn Tỉnh/Thành phố</option>`;

    data.forEach(p => {
        provinceSelect.innerHTML += `
            <option value="${p.name}" data-code="${p.code}">${p.name}</option>
        `;
    });
}

// 2. Khi chọn tỉnh → load xã/phường theo tỉnh
async function loadWardsByProvince(provinceCode) {
    if (!provinceCode) return;

    const res = await fetch(`https://provinces.open-api.vn/api/v2/p/${provinceCode}?depth=2`);
    const data = await res.json();

    // API trả về wards nằm trong data.wards
    const wards = data.wards || [];

    wardSelect.innerHTML = `<option value="">Chọn Phường/Xã</option>`;

    wards.forEach(w => {
        wardSelect.innerHTML += `
            <option value="${w.name}" data-code="${w.code}">${w.name}</option>
        `;
    });
}

// 3. Event change
provinceSelect.addEventListener("change", function () {
    const selectedOption = this.options[this.selectedIndex];
    const code = selectedOption.getAttribute("data-code");
    loadWardsByProvince(code);
});

// init
loadProvinces();