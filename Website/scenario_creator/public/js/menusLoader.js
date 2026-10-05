document.addEventListener('DOMContentLoaded', function() {
    fetch('/api/menus')
        .then(response => response.json())
        .then(menus => {
            const menusList = document.getElementById('menus-list');
            menus.forEach(menu => {
                const menuCard = document.createElement('div');
                menuCard.className = 'p-4 border rounded bg-gray-100 flex justify-between items-center';

                const menuInfo = document.createElement('div');
                menuInfo.innerHTML = `
                    <h2 class="text-xl font-bold">${menu.name}</h2>
                    <p class="text-gray-700">${menu.description || 'Aucune description disponible'}</p>
                    <p class="text-gray-600">Entrée : ${menu.appetizer || 'Non spécifié'}</p>
                    <p class="text-gray-600">Plat principal : ${menu.mainCourse || 'Non spécifié'}</p>
                    <p class="text-gray-600">Dessert : ${menu.dessert || 'Non spécifié'}</p>
                `;

                const buttonsContainer = document.createElement('div');
                buttonsContainer.className = 'space-x-4';

                const editButton = document.createElement('button');
                editButton.className = 'edit-menu text-white px-4 py-2 rounded';
                editButton.textContent = 'Modifier';
                editButton.addEventListener('click', function() {
                    window.location.href = `/views/menu-update.html?id=${menu.id}`;
                });

                const deleteButton = document.createElement('button');
                deleteButton.className = 'delete-menu text-white px-4 py-2 rounded';
                deleteButton.textContent = 'Supprimer';
                deleteButton.addEventListener('click', function() {
                    if (confirm('Êtes-vous sûr de vouloir supprimer ce menu ?')) {
                        fetch(`/api/menus/${menu.id}`, {
                            method: 'DELETE'
                        })
                        .then(response => {
                            if (response.ok) {
                                alert('Menu supprimé avec succès');
                                menuCard.remove();
                            } else {
                                alert("Une erreur s'est produite lors de la suppression du menu.");
                            }
                        })
                        .catch(error => {
                            console.error('Erreur:', error);
                            alert("Une erreur s'est produite lors de la suppression du menu.");
                        });
                    }
                });

                buttonsContainer.appendChild(editButton);
                buttonsContainer.appendChild(deleteButton);

                menuCard.appendChild(menuInfo);
                menuCard.appendChild(buttonsContainer);

                menusList.appendChild(menuCard);
            });
        })
        .catch(error => {
            console.error('Erreur:', error);
            alert("Une erreur s'est produite lors du chargement des menus.");
        });
});
