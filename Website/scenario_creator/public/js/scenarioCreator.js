import {
  addMenu,
  addDrink,
  addExtra,
} from './commonFunctions.js';

document.addEventListener('DOMContentLoaded', function () {

  document.getElementById('add-menu').addEventListener('click', addMenu);
  document.getElementById('add-drink').addEventListener('click', addDrink);
  document.getElementById('add-extra').addEventListener('click', addExtra);

  document.getElementById('save-scenario').addEventListener('click', function () {
    const drinkTypes = Array.from(document.querySelectorAll('.drink-select')).map(select => select.value);
    const duplicateDrink = drinkTypes.some((type, index) => drinkTypes.indexOf(type) !== index);
    
    if (duplicateDrink) {
      alert('Chaque boisson doit être unique. Veuillez vérifier les boissons sélectionnées.');
      return;
    }

    const extraTypes = Array.from(document.querySelectorAll('.extra-select')).map(select => select.value);
    const duplicateExtra = extraTypes.some((type, index) => extraTypes.indexOf(type) !== index);
    
    if (duplicateExtra) {
      alert('Chaque extra doit être unique. Veuillez vérifier les extras sélectionnés.');
      return;
    }

    const scenario = {
      name: document.getElementById('name').value,
      description: document.getElementById('description').value,
      mode: document.getElementById('mode').value,
      level: document.getElementById('level').value,
      allowedTime: parseFloat(document.getElementById('allowedTime').value),
      validator: {
        position: document.getElementById('position').checked,
        rotation: document.getElementById('rotation').checked,
        order: document.getElementById('order').checked
      },
      drinks: Array.from(document.querySelectorAll('.drink-item')).map(item => ({
        type: item.querySelector('.drink-select').value,
        name: item.querySelector('.drink-name').value || null
      })),
      extras: Array.from(document.querySelectorAll('.extra-select')).map(select => ({
        tag: select.value, name: select.options[select.selectedIndex].text
      })),
      menus: Array.from(document.querySelectorAll('.menu-select')).map(select => select.value)
    };

    fetch('/api/scenarios', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(scenario)
    }).then(() => alert('Scenario saved successfully!'));
  });
});
