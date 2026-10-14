/* Reusable Country -> Governorate -> City cascading select.
   Markup contract: a wrapper with [data-location-cascade] carrying the lookup URLs and
   (optionally) the previously-saved ids to preselect, containing three <select>s marked
   [data-cascade-level="country|governorate|city"]. Only the city select needs a `name`/`asp-for` —
   country and governorate exist purely to narrow the city list down. */
(function ($) {
    'use strict';

    function fillOptions($select, items, arabic, placeholder, selectedId) {
        $select.empty().append($('<option>', { value: '', text: placeholder }));
        items.forEach(function (item) {
            $select.append($('<option>', { value: item.id, text: arabic ? item.arName : item.enName }));
        });
        $select.prop('disabled', items.length === 0);
        if (selectedId) { $select.val(String(selectedId)); }
    }

    $('[data-location-cascade]').each(function () {
        var $root = $(this);
        var arabic = String($root.data('arabic')) === '1';
        var placeholder = $root.data('placeholder') || '';
        var urls = {
            country: $root.data('countriesUrl'),
            governorate: $root.data('governoratesUrl'),
            city: $root.data('citiesUrl')
        };
        var $country = $root.find('[data-cascade-level="country"]');
        var $governorate = $root.find('[data-cascade-level="governorate"]');
        var $city = $root.find('[data-cascade-level="city"]');

        function loadCities(governorateId, selectedCity) {
            if (!governorateId) {
                $city.empty().prop('disabled', true);
                return;
            }
            $.get(urls.city, { governorateId: governorateId }, function (data) {
                fillOptions($city, data, arabic, placeholder, selectedCity);
            });
        }

        function loadGovernorates(countryId, selectedGovernorate, selectedCity) {
            if (!countryId) {
                $governorate.empty().prop('disabled', true);
                $city.empty().prop('disabled', true);
                return;
            }
            $.get(urls.governorate, { countryId: countryId }, function (data) {
                fillOptions($governorate, data, arabic, placeholder, selectedGovernorate);
                if (selectedGovernorate) { loadCities(selectedGovernorate, selectedCity); }
                else { $city.empty().prop('disabled', true); }
            });
        }

        $country.on('change', function () { loadGovernorates($(this).val()); });
        $governorate.on('change', function () { loadCities($(this).val()); });

        $.get(urls.country, function (data) {
            var selectedCountry = $root.data('selectedCountry');
            fillOptions($country, data, arabic, placeholder, selectedCountry);
            if (selectedCountry) {
                loadGovernorates(selectedCountry, $root.data('selectedGovernorate'), $root.data('selectedCity'));
            }
        });
    });
})(jQuery);
