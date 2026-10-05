import {
  addMenu,
  addDrink,
  addExtra,
  addLoadedMenu,
  addLoadedDrink,
  addLoadedExtra,
  fetchDrinks,
  fetchMenus,
  fetchExtras,
} from './commonFunctions.js';

document.addEventListener('DOMContentLoaded', async function () {
    const scenarioId = new URLSearchParams(window.location.search).get('id');
    const form = document.getElementById('scenario-form');
  
    if (!scenarioId) {
      alert("Aucun scénario spécifié !");
      return;
    }
  
    async function loadScenarioData() {
      try {
        const response = await fetch(`/api/scenarios/${scenarioId}`);
        if (!response.ok) throw new Error("Erreur lors du chargement du scénario");
  
        const scenario = await response.json();
  
        document.getElementById('name').value = scenario.name || '';
        document.getElementById('description').value = scenario.description || '';
        document.getElementById('mode').value = scenario.mode || 'Learning';
        document.getElementById('level').value = scenario.level || 'Begginner';
        document.getElementById('allowedTime').value = scenario.allowed_time || 15;
        document.getElementById('position').checked = scenario.validator_position || false;
        document.getElementById('rotation').checked = scenario.validator_rotation || false;
        document.getElementById('order').checked = scenario.validator_order || false;
  
        const allMenus = await fetchMenus();
        scenario.menus.forEach(menu => addLoadedMenu(allMenus, menu.MenuId));
  
        const allDrinks = await fetchDrinks();
        scenario.drinks.forEach(drink => addLoadedDrink(allDrinks, drink.type, drink.ScenarioDrinks.name));
  
        const allExtras = await fetchExtras();
        scenario.Extras.forEach(extra => addLoadedExtra(allExtras, extra.name));
      } catch (error) {
        console.error("Erreur de chargement des données :", error);
        alert("Impossible de charger les données du scénario.");
      }
    }
  
  
    async function saveScenario() {
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

      const scenarioData = {
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
        menus: Array.from(document.querySelectorAll('.menu-select')).map(select => select.value),
        drinks: Array.from(document.querySelectorAll('.drink-item')).map(item => ({
          type: item.querySelector('.drink-select').value,
          name: item.querySelector('.drink-name').value || null
        })),
        extras: Array.from(document.querySelectorAll('.extra-select')).map(select => ({
          tag: select.value, name: select.options[select.selectedIndex].text
        }))
      };
  
      try {
        const response = await fetch(`/api/scenarios/${scenarioId}`, {
          method: 'PUT',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(scenarioData)
        });
  
        if (!response.ok) throw new Error("Erreur lors de la sauvegarde du scénario");
        alert("Scénario mis à jour avec succès !");
      } catch (error) {
        console.error("Erreur lors de la sauvegarde :", error);
        alert("Impossible de sauvegarder les modifications.");
      }
    }
  
    document.getElementById('add-menu').addEventListener('click', async () => {
      const menus = await fetchMenus();
      addMenu(menus);
    });
  
    document.getElementById('add-drink').addEventListener('click', async () => {
      const drinks = await fetchDrinks();
      addDrink(drinks);
    });
  
    document.getElementById('add-extra').addEventListener('click', async () => {
      const extras = await fetchExtras();
      addExtra(extras);
    });
  
    form.addEventListener('submit', async function (event) {
      event.preventDefault();
      await saveScenario();
    });

    document.getElementById('save-scenario').addEventListener('click', async () => {
        await saveScenario();
      });
  
    await loadScenarioData();
  });
  