import { 
    calculateRelativePosition, 
    convertCanvasRotationToQuaternion, 
    calculateAbsolutePosition, 
    hasDuplicatePlacementOrderIndex, 
    showControlPanel, 
    // updateParentSelect,
    getCutleriesCount,
    incrementCutleriesCount,
    cutleryOptions,
    utensils   
} from './commonFunctions.js';

const canvas = new fabric.Canvas('tableCanvas', { selection: true, backgroundColor: '#873b01' });

const iconContainer = document.getElementById('iconContainer');
// const parentSelect = document.getElementById('parentSelect');
const centerSelectionPanel = document.getElementById('centerSelectionPanel');
const centerSelectionButton = document.getElementById('centerSelection');
const controlPanel = document.getElementById('controlPanel');

utensils.forEach((utensil) => {
    const container = document.createElement('div');
    container.classList.add('flex', 'flex-col', 'items-center');

    const div = document.createElement('div');
    div.classList.add('w-16', 'h-16', 'bg-gray-200', 'border', 'rounded', 'flex', 'items-center', 'justify-center', 'cursor-pointer', 'hover:bg-gray-300', 'utensil-icon');
    div.style.backgroundImage = `url('${utensil.icon}')`;
    div.setAttribute('data-id', utensil.id);
    div.setAttribute('data-name', utensil.name);
    div.setAttribute('data-icon', utensil.icon);

    div.draggable = true;
    div.ondragstart = (e) => {
        e.dataTransfer.setData('id', utensil.id);
        e.dataTransfer.setData('name', utensil.name);
        e.dataTransfer.setData('icon', utensil.icon);
    };

    const label = document.createElement('span');
    label.classList.add('block', 'text-center', 'text-xs', 'mt-1', 'font-semibold');
    label.textContent = utensil.label;

    container.appendChild(div);
    container.appendChild(label);
    iconContainer.appendChild(container);
});

canvas.upperCanvasEl.addEventListener('dragover', (e) => e.preventDefault());

canvas.upperCanvasEl.addEventListener('drop', (e) => {
    e.preventDefault();

    const name = e.dataTransfer.getData('name');
    let icon = `/images/${name}.png`;
    if (name === 'GlassWine' || name === 'GlassWater') {
        icon = `/images/${name}_top.png`;
    } else if (name !== 'Plate') {
        icon = `/images/${name}_reversed.png`;
    }

    if (canvas.getObjects().some((obj) => obj.data?.name === name)) {
        alert(`L'ustensile "${name}" est déjà ajouté.`);
        return;
    }

    const rect = canvas.upperCanvasEl.getBoundingClientRect();
    const pointer = { x: e.clientX - rect.left, y: e.clientY - rect.top };
    const scale = cutleryOptions.find(item => item.value === name).scale;

    fabric.Image.fromURL(icon, (img) => {
        if (!img) return;
        img.set({
            left: pointer.x, 
            top: pointer.y,  
            originX: 'center', 
            originY: 'center', 
            scaleX: scale,
            scaleY: scale,
            hasControls: false,
            hasBorders: true,
            selectable: true,
            centeredRotation: true,
            data: { name, parent: 'Table', placementOrderIndex: getCutleriesCount() },
        });
        canvas.add(img);
        incrementCutleriesCount();
        img.on('mousedown', () => showControlPanel(img, canvas, controlPanel));
        canvas.renderAll();
    });
});

canvas.on('mouse:down', (event) => {
    if (!event.target) {
        controlPanel.classList.add('hidden');
    }
});

// canvas.on('selection:created', () => updateParentSelect(canvas, parentSelect));
// canvas.on('selection:updated', () => updateParentSelect(canvas, parentSelect));

canvas.on('selection:created', () => {
    const activeSelection = canvas.getActiveObject();
    if (activeSelection && activeSelection.type === 'activeSelection' && Array.isArray(activeSelection._objects) && activeSelection._objects.length > 1) {
        centerSelectionPanel.style.display = 'block'; 
    } else {
        centerSelectionPanel.style.display = 'none'; 
    }
});

canvas.on('selection:updated', () => {
    const activeSelection = canvas.getActiveObject();
    if (activeSelection && activeSelection.type === 'activeSelection' && Array.isArray(activeSelection._objects) && activeSelection._objects.length > 1) {
        centerSelectionPanel.style.display = 'block'; 
    } else {
        centerSelectionPanel.style.display = 'none'; 
    }
});


canvas.on('selection:cleared', () => {
    centerSelectionPanel.style.display = 'none'; 
});

centerSelectionButton.addEventListener('click', () => {
    const activeSelection = canvas.getActiveObject();
    if (activeSelection && activeSelection.type === 'activeSelection') {
        activeSelection.center();
        activeSelection.setCoords();
        canvas.renderAll();
    }
});

const saveButton = document.getElementById('save-menu');
saveButton.addEventListener('click', async () => {
    if (hasDuplicatePlacementOrderIndex(canvas)) {
        alert('Des doublons ont été détectés dans les index d’ordre. Veuillez corriger avant de sauvegarder.');
        return;
    }

    const menuName = document.querySelector('.menu-name').value.trim();
    const menuDescription = document.querySelector('.menu-description').value.trim();
    const menuAppetizer = document.querySelector('.menu-appetizer').value.trim();
    const menuMainCourse = document.querySelector('.menu-main-course').value.trim();
    const menuDessert = document.querySelector('.menu-dessert').value.trim();
    
    if (!menuName || !menuDescription || !menuAppetizer || !menuMainCourse || !menuDessert) {
        alert('Veuillez remplir tous les champs du menu.');
        return;
    }

    const canvasCenter = { x: canvas.getWidth() / 2, y: canvas.getHeight() / 2 };
    const cutleries = canvas.getObjects()
        .filter((obj) => obj.data && obj.data.name)
        .map((obj) => {
            let parentObj = canvas.getObjects().find((o) => o.data.name === obj.data.parent);
            const position = calculateRelativePosition(obj, parentObj, canvasCenter);
            const rotation = convertCanvasRotationToQuaternion(obj.angle || 0, obj.data.name, obj.data.parent);
            return {
                tag: obj.data.name,
                parent: obj.data.parent || 'Table',
                placementOrderIndex: obj.data.placementOrderIndex || 0,
                offset: { position, rotation },
            };
        });

    const menuData = {
        name: menuName,
        description: menuDescription,
        appetizer: menuAppetizer,
        mainCourse: menuMainCourse,
        dessert: menuDessert,
        cutleryToPlace: cutleries.reduce((acc, cur) => {
            acc[cur.tag] = cur;
            return acc;
        }, {}),
    };

    try {
        const menuId = new URLSearchParams(window.location.search).get('id');
        const response = await fetch(`/api/menus/${menuId}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(menuData),
        });
        if (response.ok) {
            alert('Menu sauvegardé avec succès!');
        } else {
            alert('Erreur lors de la sauvegarde du menu.');
        }
    } catch (error) {
        alert('Une erreur est survenue lors de la sauvegarde.');
        console.error(error);
    }
});

async function initializePage() {
    const menuId = new URLSearchParams(window.location.search).get('id');
    if (menuId) {
        const menuData = await fetchMenuData(menuId);
        if (menuData) {
            populateMenu(menuData);
        }
    } else {
        alert('Aucun ID de menu trouvé.');
    }
}

async function fetchMenuData(menuId) {
    try {
        const response = await fetch(`/api/menus/${menuId}`);
        if (!response.ok) {
            throw new Error(`Failed to fetch menu: ${response.statusText}`);
        }
        return await response.json();
    } catch (error) {
        console.error('Error fetching menu:', error);
        alert('Erreur lors de la récupération du menu.');
        return null;
    }
}

function populateMenu(menuData) {
    if (!menuData || !menuData.Cutleries) {
        console.error('Données de menu invalides ou manquantes');
        alert('Erreur lors de la récupération des données du menu.');
        return;
    }

    document.querySelector('.menu-name').value = menuData.name || '';
    document.querySelector('.menu-description').value = menuData.description || '';
    document.querySelector('.menu-appetizer').value = menuData.appetizer || '';
    document.querySelector('.menu-main-course').value = menuData.mainCourse || '';
    document.querySelector('.menu-dessert').value = menuData.dessert || '';

    const canvasCenter = { x: canvas.getWidth() / 2, y: canvas.getHeight() / 2 };
    const objectsToAdjust = [];
    menuData.Cutleries.forEach((cutlery) => {
        const offset = cutlery.MenuCutleries?.offset || null;
        const tag = cutlery.tag;
        let icon = `/images/${tag}.png`
        if (tag === 'GlassWine' || tag === 'GlassWater') {
            icon = `/images/${tag}_top.png`
        } else if (tag !== 'Plate') {
            icon = `/images/${tag}_reversed.png`
        }

        if (!offset || !tag) {
            console.warn(`Données manquantes pour l'ustensile : ${cutlery}`);
            return;
        }

        const scale = cutleryOptions.find(item => item.value === tag).scale;


        fabric.Image.fromURL(icon, (img) => {
            if (!img) return;

            img.set({
                left: canvasCenter.x,
                top: canvasCenter.y,
                originX: 'center', 
                originY: 'center', 
                scaleX: scale,
                scaleY: scale,
                angle: 0,
                hasControls: false,
                hasBorders: true,
                selectable: true,
                centeredRotation: true,
                data: {
                    name: tag,
                    parent: cutlery.MenuCutleries.parent || 'Table',
                    placementOrderIndex: getCutleriesCount(),
                    offset: cutlery.MenuCutleries.offset,
                },
            });

            canvas.add(img);
            objectsToAdjust.push(img);
            incrementCutleriesCount();
            img.on('mousedown', () => showControlPanel(img, canvas, controlPanel));
            canvas.renderAll();
        });
    });

    setTimeout(() => {
        console.log("adjusting cutleries");
        objectsToAdjust.forEach((obj) => {
            const { offset, parent } = obj.data;

            const parentObj = parent === 'Table'
                ? null
                : canvas.getObjects().find((o) => o.data?.name === parent);

            const absolutePosition = parentObj
            ? calculateAbsolutePosition(offset.position, parentObj, canvasCenter)
            : calculateAbsolutePosition(offset.position, null, canvasCenter);

            obj.set({
                left: absolutePosition.left,
                top: absolutePosition.top,
                angle: Math.atan2(offset.rotation.z, offset.rotation.w) * (180 / Math.PI) * 2,
            });

            console.log(offset.rotation);
            console.log(obj.angle);

            obj.setCoords(); 
        });

        canvas.renderAll(); 
    }, 100);
}

initializePage();