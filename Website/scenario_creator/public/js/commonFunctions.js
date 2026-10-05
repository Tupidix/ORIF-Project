let cutleriesCount = 0;
let menusCount = 0;

// const parentSelect = document.getElementById('parentSelect');

const menusContainer = document.getElementById('menus');
const drinksList = document.getElementById('drinks-list');
const extrasList = document.getElementById('extras-list');

export function getCutleriesCount() {
  return cutleriesCount;
}

export function incrementCutleriesCount() {
  cutleriesCount++;
}

export function decrementCutleriesCount() {
  cutleriesCount--;
}

export function calculateRelativePosition(obj, parentObj, canvasCenter) {
  const divider = 1000; 

  const objectCenterX = obj.left; 
  const objectCenterY = obj.top;
  if (obj.data.parent === "Table") {
      return {
          x: (objectCenterX - canvasCenter.x) / divider,
          y: 0.81, 
          z: -(objectCenterY - canvasCenter.y) / divider,
      };
  }

  const parentCenterX = parentObj.left;
  const parentCenterY = parentObj.top;

  return {
      x: (objectCenterX - parentCenterX) / divider,
      y: 0,
      z: -(objectCenterY - parentCenterY) / divider,
  };
}

export function convertCanvasRotationToQuaternion(angleInDegrees, objName, parentName) {
  if ((objName === "GlassWater" || objName === "GlassWine") && parentName === "Plate") {
    return { x: 1, y: 0, z: 0, w: 6.123233995736766e-17 };
  } else if ((objName === "GlassWater" || objName === "GlassWine") && parentName === "Table") {
    return { x: 0.7071, y: 0, z: 0, w: 0.7071 };
  } else if ((objName === "Plate" || parentName === "Table") && angleInDegrees === 0) {
    return { x: -0.7071, y: 0, z: 0, w: 0.7071 };
  } else if (parentName === "Table" && angleInDegrees !== 0) {
    // Quaternion pour X = -90°
    const xRadians = (-90 * Math.PI) / 180; 
    const qx = { 
      x: Math.sin(xRadians / 2), 
      y: 0, 
      z: 0, 
      w: Math.cos(xRadians / 2) 
    };

    // Quaternion pour Z = angleInDegrees
    const zRadians = (angleInDegrees * Math.PI) / 180; 
    const qz = { 
      x: 0, 
      y: 0, 
      z: Math.sin(zRadians / 2), 
      w: Math.cos(zRadians / 2) 
    };

    // On multiplie qx par qz
    return multiplyQuaternions(qx, qz);
  } else {
    const radians = (angleInDegrees * Math.PI) / 180;
    return { x: 0, y: 0, z: Math.sin(radians / 2), w: Math.cos(radians / 2) };
  }
}

function multiplyQuaternions(q1, q2) {
  return {
    x: q1.w * q2.x + q1.x * q2.w + q1.y * q2.z - q1.z * q2.y,
    y: q1.w * q2.y - q1.x * q2.z + q1.y * q2.w + q1.z * q2.x,
    z: q1.w * q2.z + q1.x * q2.y - q1.y * q2.x + q1.z * q2.w,
    w: q1.w * q2.w - q1.x * q2.x - q1.y * q2.y - q1.z * q2.z
  };
}

export function calculateAbsolutePosition(relativePosition, parentObj, canvasCenter) {
  const divider = 1000; 

  if (!parentObj || parentObj.data.parent === "Table") {
      return {
          left: (relativePosition.x * divider) + canvasCenter.x,
          top: canvasCenter.y - (relativePosition.z * divider),
      };
  }

  const parentCenterX = parentObj.left; 
  const parentCenterY = parentObj.top;

  return {
      left: (relativePosition.x * divider) + parentCenterX,
      top: parentCenterY - (relativePosition.z * divider),
  };
}

export function hasDuplicatePlacementOrderIndex(canvas) {
  const indexes = canvas.getObjects()
      .filter((obj) => obj.data && obj.data.placementOrderIndex !== undefined)
      .map((obj) => obj.data.placementOrderIndex);

  return indexes.length !== new Set(indexes).size;
}

export function showControlPanel(object, canvas, controlPanel) {
  controlPanel.classList.remove('hidden');

  const cutleryName = document.getElementById('controlPanelCutleryName');
  const posXInput = document.getElementById('posX');
  const posYInput = document.getElementById('posY');
  const rotationInput = document.getElementById('rotation');
  const placementOrderInput = document.getElementById('placementOrder');
  const buttonDelete = document.getElementById('deleteObject');

  const centerX = object.left; 
  const centerY = object.top; 

  const cutleryOption = cutleryOptions.find(option => option.value === object.data.name);
  cutleryName.textContent = cutleryOption.text;
  posXInput.value = Math.round(centerX || 0);
  posYInput.value = Math.round(centerY || 0);
  rotationInput.value = Math.round(object.angle || 0);
  placementOrderInput.value = object.data.placementOrderIndex;

  // updateParentSelect(canvas, parentSelect, object);

  posXInput.oninput = () => {
      object.set({ left: parseInt(posXInput.value, 10) });
      canvas.renderAll();
  };

  posYInput.oninput = () => {
      object.set({ top: parseInt(posYInput.value, 10) });
      canvas.renderAll();
  };

  rotationInput.oninput = () => {
      object.set({ angle: parseInt(rotationInput.value, 10) });
      canvas.renderAll();
  };

  placementOrderInput.oninput = () => {
      object.data.placementOrderIndex = parseInt(placementOrderInput.value, 10) || 0;
      canvas.renderAll();
  };

  buttonDelete.onclick = () => {
      canvas.remove(object); 
      controlPanel.classList.add('hidden'); 
      canvas.renderAll(); 
      decrementCutleriesCount();
  };

  // parentSelect.onchange = () => {
  //   object.data.parent = parentSelect.value;
  // };

  object.on('moving', () => {
      posXInput.value = Math.round(object.left || 0); 
      posYInput.value = Math.round(object.top || 0);  
  });

  object.on('rotating', () => {
      rotationInput.value = Math.round(object.angle || 0);
  });
}

// export function updateParentSelect(canvas, parentSelect, currentObject = null) {
//   parentSelect.innerHTML = '<option value="Table">Table</option>';
//   canvas.getObjects()
//       .filter((obj) => obj.data?.name && obj !== currentObject)
//       .forEach((obj) => {
//           const option = document.createElement('option');
//           option.value = obj.data.name;
//           const cutleryOption = cutleryOptions.find(option => option.value === obj.data.name);
//           option.textContent = cutleryOption ? cutleryOption.text : obj.data.name;
//           parentSelect.appendChild(option);
//       });

//   if (currentObject && currentObject.data.parent) {
//       parentSelect.value = currentObject.data.parent;
//   }
// }

export const cutleryOptions = [
  { value: 'Plate', text: 'Assiette', scale : 0.75 },
  { value: 'ButterKnife', text: 'Couteau à beurre', scale : 0.2 },
  { value: 'CarvingFork', text: 'Fourchette à découper', scale : 0.4 },
  { value: 'DessertFork', text: 'Fourchette à dessert', scale : 0.225 },
  { value: 'DessertKnife', text: 'Couteau à dessert', scale : 0.275 },
  { value: 'DessertSpoon', text: 'Cuillère à dessert', scale : 0.225 },
  { value: 'DinnerFork', text: 'Fourchette à diner', scale : 0.3 },
  { value: 'DinnerKnife', text: 'Couteau à diner', scale : 0.4 },
  { value: 'DinnerSpoon', text: 'Cuillère à diner', scale : 0.3 },
  { value: 'FishFork', text: 'Fourchette à poisson', scale : 0.275 },
  { value: 'FishKnife', text: 'Couteau à poisson', scale : 0.275 },
  { value: 'FruitFork', text: 'Fourchette à fruit', scale : 0.2 },
  { value: 'FruitKnife', text: 'Couteau à fruit', scale : 0.25 },
  { value: 'FruitSpoon', text: 'Cuillère à fruit', scale : 0.175 },
  { value: 'GlassWater', text: 'Verre à eau', scale : 0.25 },
  { value: 'GlassWine', text: 'Verre à vin', scale : 0.25 },
  { value: 'SaladFork', text: 'Fourchette à salade', scale : 0.25 },
  { value: 'SteakKnife', text: 'Couteau à viande', scale : 0.4 },
  { value: 'TeaSpoon', text: 'Cuillère à thé', scale : 0.175 },
];

export const utensils = cutleryOptions.map((cutlery, index) => ({
  id: index + 1,
  name: cutlery.value,
  icon: `/images/${cutlery.value}.png`,
  label: cutlery.text,
  scale : cutlery.scale
}));

export async function fetchDrinks() {
  try {
    const response = await fetch('/api/drinks');
    if (!response.ok) throw new Error('Erreur lors de la récupération des boissons');
    return await response.json();
  } catch (error) {
    console.error('Erreur:', error);
    return [];
  }
}

export async function fetchMenus() {
  try {
    const response = await fetch('/api/menus');
    if (!response.ok) throw new Error('Erreur lors de la récupération des menus');
    return await response.json();
  } catch (error) {
    console.error('Erreur:', error);
    return [];
  }
}

export async function fetchExtras() {
  try {
    const response = await fetch('/api/extras');
    if (!response.ok) throw new Error('Erreur lors de la récupération des extras');
    return await response.json();
  } catch (error) {
    console.error('Erreur:', error);
    return [];
  }
}

export async function addMenu() {
  const availableMenus = await fetchMenus();
  if (availableMenus.length === 0) {
    alert('Aucun menu disponible.');
    return;
  }

  const menuDiv = document.createElement('div');
  menuDiv.className = 'menu p-4 border rounded bg-gray-100 my-2';
  menuDiv.innerHTML = `
    <label class="block text-gray-700">Menu :</label>
    <select class="menu-select w-full px-3 py-2 border rounded">
      ${availableMenus.map(menu => `<option value="${menu.id}">${menu.name}</option>`).join('')}
    </select>
    <button type="button" class="remove-menu text-white px-4 py-2 rounded mt-2">Retirer le menu</button>
  `;

  menusContainer.appendChild(menuDiv);

  menuDiv.querySelector('.remove-menu').addEventListener('click', function () {
    menuDiv.remove();
    menusCount--;
    HandleUpdateMenuCount();
  });

  menusCount++;
  HandleUpdateMenuCount();
}

export function HandleUpdateMenuCount() {
  if (menusCount == 2) {
    document.getElementById('add-menu').classList.add("hidden");
  } else {
    document.getElementById('add-menu').classList.remove("hidden");
  }
}

export async function addDrink() {
  const availableDrinks = await fetchDrinks();
  if (availableDrinks.length === 0) {
    alert('Aucune boisson disponible.');
    return;
  }

  const drinkItem = document.createElement('div');
  drinkItem.className = 'drink-item flex items-center space-x-4';

  const drinkSelect = document.createElement('select');
  drinkSelect.className = 'drink-select w-full px-3 py-2 border rounded';
  drinkSelect.innerHTML = availableDrinks.map(drink => `<option value="${drink.type}">${drink.type}</option>`).join('');

  const drinkNameInput = document.createElement('input');
  drinkNameInput.type = 'text';
  drinkNameInput.className = 'drink-name w-full px-3 py-2 border rounded hidden';
  drinkNameInput.placeholder = 'Nom de la bouteille';

  const removeButton = document.createElement('button');
  removeButton.type = 'button';
  removeButton.className = 'remove-drink px-4 py-2 rounded text-white';
  removeButton.textContent = 'Retirer';

  drinkItem.appendChild(drinkSelect);
  drinkItem.appendChild(drinkNameInput);
  drinkItem.appendChild(removeButton);
  drinksList.appendChild(drinkItem);

  drinkSelect.addEventListener('change', function () {
    if (drinkSelect.value === 'Vin blanc' || drinkSelect.value === 'Vin rouge') {
      drinkNameInput.classList.remove('hidden');
    } else {
      drinkNameInput.classList.add('hidden');
    }
  });

  removeButton.addEventListener('click', function () {
    drinkItem.remove();
  });
}

export async function addExtra() {
  const availableExtras = await fetchExtras();
  if (availableExtras.length === 0) {
    alert('Aucun extra disponible.');
    return;
  }

  const extraDiv = document.createElement('div');
  extraDiv.className = 'extra-item flex items-center space-x-4';

  const extraSelect = document.createElement('select');
  extraSelect.className = 'extra-select w-full px-3 py-2 border rounded';
  extraSelect.innerHTML = availableExtras.map(extra => `<option value="${extra.tag}">${extra.name}</option>`).join('');

  const removeButton = document.createElement('button');
  removeButton.type = 'button';
  removeButton.className = 'remove-extra px-4 py-2 rounded text-white';
  removeButton.textContent = 'Retirer';

  extraDiv.appendChild(extraSelect);
  extraDiv.appendChild(removeButton);
  extrasList.appendChild(extraDiv);

  removeButton.addEventListener('click', function () {
    extraDiv.remove();
  });
}

export function getMenusCount() {
  return menusCount;
}

export function incrementMenusCount() {
  menusCount++;
}

export function decrementMenusCount() {
  menusCount--;
}

export async function addLoadedMenu(availableMenus, selectedMenuId = null) {
  if (!availableMenus || availableMenus.length === 0) {
    alert('Aucun menu disponible.');
    return;
  }
  const menuDiv = document.createElement('div');
  menuDiv.className = 'menu p-4 border rounded bg-gray-100 my-2';
  menuDiv.innerHTML = `
    <label class="block text-gray-700">Menu :</label>
    <select class="menu-select w-full px-3 py-2 border rounded">
      ${availableMenus.map(menu =>
        `<option value="${menu.id}" ${menu.id === selectedMenuId ? 'selected' : ''}>${menu.name}</option>`
      ).join('')}
    </select>
    <button type="button" class="remove-menu px-4 py-2 rounded mt-2 text-white">Retirer le menu</button>
  `;
  menusContainer.appendChild(menuDiv);

  menuDiv.querySelector('.remove-menu').addEventListener('click', function () {
    menuDiv.remove();
    menusCount--;
    HandleUpdateMenuCount();
  });

  menusCount++;
  HandleUpdateMenuCount();
}

export async function addLoadedDrink(availableDrinks, selectedDrinkType = '', drinkName = '') {
  if (!availableDrinks || availableDrinks.length === 0) {
    alert('Aucune boisson disponible.');
    return;
  }

  const drinkDiv = document.createElement('div');
  drinkDiv.className = 'drink-item flex items-center space-x-4';

  const drinkSelect = document.createElement('select');
  drinkSelect.className = 'drink-select w-full px-3 py-2 border rounded';
  drinkSelect.innerHTML = availableDrinks.map(drink =>
    `<option value="${drink.type}" ${drink.type === selectedDrinkType ? 'selected' : ''}>${drink.type}</option>`
  ).join('');

  const drinkNameInput = document.createElement('input');
  drinkNameInput.type = 'text';
  drinkNameInput.value = drinkName || '';
  drinkNameInput.placeholder = 'Nom de la bouteille';
  drinkNameInput.className = `drink-name w-full px-3 py-2 border rounded ${selectedDrinkType === 'Vin blanc' || selectedDrinkType === 'Vin rouge' ? '' : 'hidden'}`;

  const removeButton = document.createElement('button');
  removeButton.type = 'button';
  removeButton.className = 'remove-drink px-4 py-2 rounded text-white';
  removeButton.textContent = 'Retirer';

  drinkDiv.appendChild(drinkSelect);
  drinkDiv.appendChild(drinkNameInput);
  drinkDiv.appendChild(removeButton);
  drinksList.appendChild(drinkDiv);

  drinkSelect.addEventListener('change', () => {
    if (drinkSelect.value === 'Vin blanc' || drinkSelect.value === 'Vin rouge') {
      drinkNameInput.classList.remove('hidden');
    } else {
      drinkNameInput.classList.add('hidden');
    }
  });

  removeButton.addEventListener('click', () => drinkDiv.remove());
}

export async function addLoadedExtra(availableExtras, selectedExtraName = '') {
  if (!availableExtras || availableExtras.length === 0) {
    alert('Aucun extra disponible.');
    return;
  }

  const extraDiv = document.createElement('div');
  extraDiv.className = 'extra-item flex items-center space-x-4';
  extraDiv.innerHTML = `
    <select class="extra-select w-full px-3 py-2 border rounded">
      ${availableExtras.map(extra =>
        `<option value="${extra.tag}" ${extra.name === selectedExtraName ? 'selected' : ''}>${extra.name}</option>`
      ).join('')}
    </select>
    <button type="button" class="remove-extra px-4 py-2 rounded text-white">Retirer</button>
  `;
  extrasList.appendChild(extraDiv);

  extraDiv.querySelector('.remove-extra').addEventListener('click', () => extraDiv.remove());
}
