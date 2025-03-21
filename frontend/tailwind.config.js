module.exports = {
	content: ['./src/**/*.{html,ts}', './projects/**/*.{html,ts}'],
	theme: {
		extend: {
			colors: {
				primary: '#3f51b5', // Couleurs Material par défaut
				accent: '#ff4081',
			},
		},
	},
	corePlugins: {
		preflight: false, // Désactive les styles de base pour éviter les conflits
	},
};
